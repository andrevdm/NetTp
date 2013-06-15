using System;
using System.Threading.Tasks;

namespace Avdm.NetTp.Messaging
{
    public interface INetTpMessageBusImpl
    {
        void PublishEvent<TEvent>( Action<Action<TEvent>> publisher ) where TEvent : INetTpEventMessage;
        void PublishEvent<TEvent>(  string topic, Action<Action<TEvent>> publisher ) where TEvent : INetTpEventMessage;
        void SubscribeToEvent<TEvent>( string subscriberId, Action<TEvent> handler ) where TEvent : INetTpEventMessage;
        void SubscribeToEvent<TEvent>( string subscriberId, string topic, Action<TEvent> handler ) where TEvent : INetTpEventMessage;

        void PublishCommand<TCommand>( Action<Action<TCommand>> publisher ) where TCommand : INetTpCommandMessage;
        void PublishCommand<TCommand>( string topic, Action<Action<TCommand>> publisher ) where TCommand : INetTpCommandMessage;
        void SubscribeToCommand<TCommand>( string subscriberId, Action<TCommand> handler ) where TCommand : INetTpCommandMessage;
        void SubscribeToCommand<TCommand>( string subscriberId, string topic, Action<TCommand> handler ) where TCommand : INetTpCommandMessage;
        void SubscribeToCommandAsync<TCommand>( string subscriberId, Func<TCommand, Task> handler ) where TCommand : INetTpCommandMessage;

        void Rpc<TRequest, TResponse>( Action<Func<TRequest, TResponse>> caller )
            where TRequest : INetTpRpcRequestMessage
            where TResponse : INetTpRpcResponseMessage;
        void SubscribeToRpc<TRequest, TResponse>( string subscriberId, Func<TRequest, TResponse> handler )
            where TRequest : INetTpRpcRequestMessage
            where TResponse : INetTpRpcResponseMessage;
    }
}