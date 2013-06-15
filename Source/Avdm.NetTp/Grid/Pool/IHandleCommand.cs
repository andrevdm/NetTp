using Avdm.NetTp.Messaging;

namespace Avdm.NetTp.Grid.Pool
{
    public interface IHandleCommand
    {
    }

    public interface IHandleCommand<TCommand> : IHandleCommand
        where TCommand : INetTpCommandMessage, new()
    {
        void HandleCommand( TCommand command );
    }
}