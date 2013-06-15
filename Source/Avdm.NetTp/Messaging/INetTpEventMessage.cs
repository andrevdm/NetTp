using System;

namespace Avdm.NetTp.Messaging
{
    public interface INetTpEventMessage : INetTpMessage
    {
    }

    [Serializable]
    public abstract class NetTpEventMessage : NetTpMessage, INetTpEventMessage
    {
    }
}