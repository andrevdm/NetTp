using System;
using System.Threading;
using System.Threading.Tasks;
using Avdm.Core.Di;
using Avdm.NetTp.Core;
using EasyNetQ;
using StructureMap;

namespace Avdm.NetTp.Messaging
{
    public class EasyNetQBusImpl : INetTpMessageBusImpl
    {
        private static readonly IBus g_bus;
        private readonly IClock m_clock;

        static EasyNetQBusImpl()
        {
            g_bus = RabbitHutch.CreateBus(
                "host=localhost", //TODO from NetTp config, also randomise server order
                reg =>
                {
                    reg.Register<IEasyNetQLogger>( _ => ObjectFactory.GetInstance<IEasyNetQLogger>() );
                } );
        }

        public EasyNetQBusImpl()
        {
            m_clock = ObjectFactory.GetInstance<IClock>();
        }

        public void PublishEvent<TEvent>( Action<Action<TEvent>> publisher ) where TEvent : INetTpEventMessage
        {
            PublishEvent<TEvent>( null, publisher );
        }

        public void PublishEvent<TEvent>( string topic, Action<Action<TEvent>> publisher ) where TEvent : INetTpEventMessage
        {
            Publish<TEvent>( publisher, topic );
        }

        public void SubscribeToEvent<TEvent>( string subscriberId, Action<TEvent> handler ) where TEvent : INetTpEventMessage
        {
            SubscribeToEvent( subscriberId, null, handler );
        }

        public void SubscribeToEvent<TEvent>( string subscriberId, string topic, Action<TEvent> handler ) where TEvent : INetTpEventMessage
        {
            //---------------------------------------------------------------------------------------------------
            // Prefix with 'evt_' so that command and event messages are never sent to one another's handlers
            // In RabbitMq if the subscriber IDs are different then you get a fanout distribution (i.e. each
            // handler gets all messages). So the caller should use a uniquie name if they want this behaviour
            //---------------------------------------------------------------------------------------------------
            subscriberId = "evt_" + topic + "_" + subscriberId;
            g_bus.Subscribe<TEvent>(
                subscriberId,
                evt => HandleEventMessage( evt, handler ),
                c =>
                {
                    if( !string.IsNullOrWhiteSpace( topic ) )
                    {
                        c.WithTopic( topic );
                    }
                } );
        }

        private void HandleEventMessage<TEvent>( TEvent evt, Action<TEvent> handler ) where TEvent : INetTpEventMessage
        {
            if( (evt.ExpireAt == null) || (m_clock.Now < evt.ExpireAt) )
            {
                handler( evt );
            }
        }

        public void PublishCommand<TCommand>( Action<Action<TCommand>> publisher ) where TCommand : INetTpCommandMessage
        {
            PublishCommand<TCommand>( null, publisher );
        }

        public void PublishCommand<TCommand>( string topic, Action<Action<TCommand>> publisher ) where TCommand : INetTpCommandMessage
        {
            Publish<TCommand>( publisher, topic );
        }

        public void SubscribeToCommand<TCommand>( string subscriberId, Action<TCommand> handler ) where TCommand : INetTpCommandMessage
        {
            SubscribeToCommand<TCommand>( subscriberId, null, handler );
        }

        public void SubscribeToCommand<TCommand>( string subscriberId, string topic, Action<TCommand> handler ) where TCommand : INetTpCommandMessage
        {
            //---------------------------------------------------------------------------------------------------
            // Prefix with 'cmd_' so that command and event messages are never sent to one another's handlers
            // In RabbitMq if the subscriber IDs are the same then you get work distribution
            // So here the topic name of the message is used as the subscriber ID so that it is always the same
            // for a message type
            //---------------------------------------------------------------------------------------------------
            subscriberId = "cmd_" + topic;
            g_bus.Subscribe<TCommand>( 
                subscriberId, 
                c => HandleCommandMessage( c, handler ), 
                c =>
                {
                    if( !string.IsNullOrWhiteSpace( topic ) )
                    {
                        c.WithTopic( topic );
                    }
                } );
        }

        public void SubscribeToCommandAsync<TCommand>( string subscriberId, Func<TCommand, Task> handler ) where TCommand : INetTpCommandMessage //TODO fix
        {
            string topic = null;

            //---------------------------------------------------------------------------------------------------
            // Prefix with 'cmd_' so that command and event messages are never sent to one another's handlers
            // In RabbitMq if the subscriber IDs are the same then you get work distribution
            // So here the topic name of the message is used as the subscriber ID so that it is always the same
            // for a message type
            //---------------------------------------------------------------------------------------------------
            subscriberId = "cmd_" + topic;
            g_bus.SubscribeAsync<TCommand>( 
                subscriberId,
                c => HandleCommandMessageAsync( c, handler ), 
                c =>
                {
                    if( !string.IsNullOrWhiteSpace( topic ) )
                    {
                        c.WithTopic( topic );
                    }
                } );
        }

        private Task HandleCommandMessageAsync<TCommand>( TCommand cmd, Func<TCommand, Task> handler ) where TCommand : INetTpCommandMessage
        {
            if( (cmd.ExpireAt == null) || (m_clock.Now < cmd.ExpireAt) )
            {
                return handler( cmd );
            }

            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult( null );
            return tcs.Task;
        }
        
        private void HandleCommandMessage<TCommand>( TCommand cmd, Action<TCommand> handler ) where TCommand : INetTpCommandMessage
        {
            if( (cmd.ExpireAt == null) || (m_clock.Now < cmd.ExpireAt) )
            {
                handler( cmd );
            }
        }

        private void Publish<T>( Action<Action<T>> publisher, string topic = null )
        {
            using( var channel = g_bus.OpenPublishChannel() )
            {
                publisher( message => channel.Publish( message, c =>
                    {
                        if( !string.IsNullOrWhiteSpace( topic ) )
                        {
                            c.WithTopic( topic );
                        }
                    } ) );
            }
        }

        public void Rpc<TRequest, TResponse>( Action<Func<TRequest, TResponse>> caller )
            where TRequest : INetTpRpcRequestMessage
            where TResponse : INetTpRpcResponseMessage
        {
            using( IPublishChannel channel = g_bus.OpenPublishChannel() )
            {
                caller( req =>
                    {
                        var wait = new ManualResetEventSlim();

                        //TODO use cancelation token + timeout
                        Task<TResponse> task = channel.RequestAsync<TRequest, TResponse>( req ).ContinueWith( t =>
                            {
                                wait.Set();
                                return t.Result;
                            } );

                        wait.Wait();

                        return task.Result;
                    } );
            }
        }

        public void SubscribeToRpc<TRequest, TResponse>( string subscriberId, Func<TRequest, TResponse> handler )
            where TRequest : INetTpRpcRequestMessage
            where TResponse : INetTpRpcResponseMessage
        {
            g_bus.Respond<TRequest, TResponse>( handler );
        }
    }
}