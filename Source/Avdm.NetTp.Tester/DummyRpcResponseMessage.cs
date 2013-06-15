using Avdm.NetTp.Messaging;

namespace Avdm.NetTp.Tester
{
    public class DummyRpcResponseMessage : NetTpRpcResponseMessage
    {
        public DummyRpcResponseMessage( string message )
        {
            ResponseMessage = message;
        }

        public string ResponseMessage { get; set; }
    }
}