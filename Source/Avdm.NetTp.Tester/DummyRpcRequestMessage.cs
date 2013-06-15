using Avdm.NetTp.Messaging;

namespace Avdm.NetTp.Tester
{
    public class DummyRpcRequestMessage : NetTpRpcRequestMessage
    {
        public DummyRpcRequestMessage( string message )
        {
            RequestMessage = message;
        }

        public string RequestMessage { get; set; }
    }
}