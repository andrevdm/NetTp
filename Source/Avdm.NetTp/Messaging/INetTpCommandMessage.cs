using System;

namespace Avdm.NetTp.Messaging
{
    public interface INetTpCommandMessage : INetTpMessage
    {
    }

    [Serializable]
    public abstract class NetTpCommandMessage : NetTpMessage, INetTpCommandMessage
    {
    }
}