using System;
using System.Threading.Tasks;
using StructureMap;

namespace Avdm.NetTp.Messaging
{
    public class NetTpMessageBus : INetTpMessageBus
    {
        private readonly INetTpMessageBusImpl m_bus; 

        public NetTpMessageBus()
        {
            m_bus = ObjectFactory.GetInstance<INetTpMessageBusImpl>();
        }

        public void PublishEvent<TEvent>( string topic, TEvent message ) where TEvent : INetTpEventMessage
        {
            m_bus.PublishEvent<TEvent>( topic, p => p( message ) );
        }

        public void PublishEvent<TEvent>( TEvent message ) where TEvent : INetTpEventMessage
        {
            m_bus.PublishEvent<TEvent>( p => p( message ) );
        }

        public void PublishEvent<TEvent>( Action<Action<TEvent>> publisher ) where TEvent : INetTpEventMessage
        {
            m_bus.PublishEvent( publisher );
        }

        public void PublishEvent<TEvent>( string topic, Action<Action<TEvent>> publisher ) where TEvent : INetTpEventMessage
        {
            m_bus.PublishEvent( topic, publisher );
        }

        public void SubscribeToEvent<TEvent>( string subscriberId, Action<TEvent> handler ) where TEvent : INetTpEventMessage
        {
            m_bus.SubscribeToEvent<TEvent>( subscriberId, handler );
        }

        public void SubscribeToEvent<TEvent>( string subscriberId, string topic, Action<TEvent> handler ) where TEvent : INetTpEventMessage
        {
            m_bus.SubscribeToEvent<TEvent>( subscriberId, topic, handler );
        }

        public void PublishCommand<TCommand>( TCommand message ) where TCommand : INetTpCommandMessage
        {
            m_bus.PublishCommand<TCommand>( p => p( message ) );
        }

        public void PublishCommand<TCommand>( string topic, TCommand message ) where TCommand : INetTpCommandMessage
        {
            m_bus.PublishCommand<TCommand>( topic, p => p( message ) );
        }

        public void PublishCommand<TCommand>( Action<Action<TCommand>> publisher ) where TCommand : INetTpCommandMessage
        {
            m_bus.PublishCommand( publisher );
        }

        public void PublishCommand<TCommand>( string topic, Action<Action<TCommand>> publisher ) where TCommand : INetTpCommandMessage
        {
            m_bus.PublishCommand( topic, publisher );
        }

        public void SubscribeToCommand<TCommand>( string subscriberId, Action<TCommand> handler ) where TCommand : INetTpCommandMessage
        {
            m_bus.SubscribeToCommand<TCommand>( subscriberId, handler );
        }

        public void SubscribeToCommandAsync<TCommand>( string subscriberId, Func<TCommand, Task> handler ) where TCommand : INetTpCommandMessage
        {
            m_bus.SubscribeToCommandAsync<TCommand>( subscriberId, handler );
        }

        public void SubscribeToCommand<TCommand>( string subscriberId, string topic, Action<TCommand> handler ) where TCommand : INetTpCommandMessage
        {
            m_bus.SubscribeToCommand<TCommand>( subscriberId, topic, handler );
        }

        public TResponse Rpc<TRequest, TResponse>( TRequest message )
            where TRequest : INetTpRpcRequestMessage
            where TResponse : INetTpRpcResponseMessage
        {
            var resp = default( TResponse );
            m_bus.Rpc<TRequest, TResponse>( c => resp = c( message ) );
            return resp;
        }

        public void Rpc<TRequest, TResponse>( Action<Func<TRequest, TResponse>> caller )
            where TRequest : INetTpRpcRequestMessage
            where TResponse : INetTpRpcResponseMessage
        {
            m_bus.Rpc( caller );
        }

        public void SubscribeToRpc<TRequest, TResponse>( string subscriberId, Func<TRequest, TResponse> handler )
            where TRequest : INetTpRpcRequestMessage
            where TResponse : INetTpRpcResponseMessage
        {
            m_bus.SubscribeToRpc<TRequest, TResponse>( subscriberId, handler );
        }
    }
}
