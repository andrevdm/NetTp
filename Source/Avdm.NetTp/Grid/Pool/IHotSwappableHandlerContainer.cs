using System;
using Avdm.NetTp.Messaging;

namespace Avdm.NetTp.Grid.Pool
{
    public interface IHotSwappableHandlerContainer
    {
        void RegisterCommandHanlder<TCommand, TCommandHandler>()
            where TCommand : INetTpCommandMessage, new()
            where TCommandHandler : IHandleCommand<TCommand>;

        void RegisterCommandHanlder( Type commandType, Type handlerType );
    }
}