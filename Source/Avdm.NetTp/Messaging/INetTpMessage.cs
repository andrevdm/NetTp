using System;

namespace Avdm.NetTp.Messaging
{
    public interface INetTpMessage
    {
        Guid Id { get; }
        DateTime CreatedAt { get; }
        DateTime? ExpireAt { get; }
    }

    [Serializable]
    public abstract class NetTpMessage : INetTpMessage
    {
        protected NetTpMessage()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;
            ExpireAt = null;
        }

        public Guid Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ExpireAt { get; set; }
    }
}