using System;

namespace Avdm.NetTp.Messaging
{
    public interface INetTpRpcResponseMessage : INetTpMessage
    {
    }

    [Serializable]
    public abstract class NetTpRpcResponseMessage : NetTpMessage, INetTpRpcResponseMessage
    {
    }

}