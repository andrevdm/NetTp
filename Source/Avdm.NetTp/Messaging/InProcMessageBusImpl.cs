/*using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NetTp.Messaging
{
    public class InProcMessageBusImpl : INetTpMessageBusImpl
    {
        private readonly ConcurrentDictionary<Type, Dictionary<string, Action<INetTpEventMessage>>> m_eventRegistrations = new ConcurrentDictionary<Type, Dictionary<string, Action<INetTpEventMessage>>>();
        private readonly ConcurrentDictionary<Type, Action<INetTpCommandMessage>> m_commandRegistrations = new ConcurrentDictionary<Type, Action<INetTpCommandMessage>>();
        private readonly ConcurrentDictionary<Tuple<Type, Type>, Func<: NetTpRpcRequestMessage, INetTpRpcResponseMessage>> m_queryRegistrations = new ConcurrentDictionary<Tuple<Type, Type>, Func<: NetTpRpcRequestMessage, INetTpRpcResponseMessage>>();

        public void PublishEvent<TEvent>( Action<Action<TEvent>> publisher ) where TEvent : NetTpEventMessage
        {
            var handlers = m_eventRegistrations[typeof( TEvent )].Values;

            foreach( Action<INetTpEventMessage> h in handlers )
            {
                var handler = h;
                publisher( message => handler( (TEvent)message ) );
            }
        }

        public void PublishEvent<TEvent>( string topic, Action<Action<TEvent>> publisher ) where TEvent : NetTpEventMessage
        {
            throw new NotImplementedException();
        }

        public void SubscribeToEvent<TEvent>( string subscriberId, Action<TEvent> handler ) where TEvent : NetTpEventMessage
        {
            Dictionary<string, Action<INetTpEventMessage>> handlers;

            if( !m_eventRegistrations.TryGetValue( typeof( TEvent ), out handlers ) )
            {
                handlers = new Dictionary<string, Action<INetTpEventMessage>>();
                m_eventRegistrations[typeof( TEvent )] = handlers;
            }

            handlers[subscriberId] = e => handler( (TEvent)e );
        }

        public void SubscribeToEvent<TEvent>( string subscriberId, string topic, Action<TEvent> handler ) where TEvent : NetTpEventMessage
        {
            throw new NotImplementedException();
        }

        public void PublishCommand<TCommand>( Action<Action<TCommand>> publisher ) where TCommand : NetTpCommandMessage
        {
            Action<INetTpCommandMessage> handler;

            if( m_commandRegistrations.TryGetValue( typeof( TCommand ), out handler ) )
            {
                publisher( message => handler( message ) );
            }
        }

        public void PublishCommand<TCommand>( string topic, Action<Action<TCommand>> publisher ) where TCommand : NetTpCommandMessage
        {
            throw new NotImplementedException();
        }

        public void SubscribeToCommand<TCommand>( string subscriberId, Action<TCommand> handler ) where TCommand : NetTpCommandMessage
        {
            if( m_commandRegistrations.ContainsKey( typeof( TCommand ) ) )
            {
                throw new InvalidOperationException( "Already registered" );
            }

            m_commandRegistrations[typeof( TCommand )] = m => handler( (TCommand)m );
        }

        public void SubscribeToCommand<TCommand>( string subscriberId, string topic, Action<TCommand> handler ) where TCommand : NetTpCommandMessage
        {
            throw new NotImplementedException();
        }

        public void Rpc<TRequest, TResponse>( Action<Func<TRequest, TResponse>> caller )
            where TRequest : NetTpRpcRequestMessage
            where TResponse : NetTpRpcResponseMessage
        {
            Func<: NetTpRpcRequestMessage, INetTpRpcResponseMessage> handler;

            if( m_queryRegistrations.TryGetValue( new Tuple<Type, Type>( typeof(TRequest), typeof(TResponse) ), out handler ) )
            {
                caller( request => (TResponse)handler( (TRequest)request ) );
            }
        }

        public void SubscribeToRpc<TRequest, TResponse>( string subscriberId, Func<TRequest, TResponse> handler )
            where TRequest : NetTpRpcRequestMessage
            where TResponse : NetTpRpcResponseMessage
        {
            var key = new Tuple<Type, Type>( typeof( TRequest ), typeof( TResponse ) );

            if( m_queryRegistrations.ContainsKey( key ) )
            {
                throw new InvalidOperationException( "Already registered" );
            }

            m_queryRegistrations[key] = req => (TResponse)handler( (TRequest)req );
        }
    }
}*/