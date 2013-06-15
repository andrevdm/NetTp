using System;

namespace Avdm.NetTp.Messaging
{
    public interface INetTpRpcRequestMessage : INetTpMessage
    {
    }

    [Serializable]
    public abstract class NetTpRpcRequestMessage : NetTpMessage, INetTpRpcRequestMessage
    {
    }
}