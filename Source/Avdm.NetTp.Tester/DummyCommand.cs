using System;
using Avdm.NetTp.Messaging;

namespace Avdm.NetTp.Tester
{
    [Serializable]                       
    public class DummyCommand : NetTpCommandMessage
    {
        public string Message { get; set; }
    }

    [Serializable]                       
    public class DummyCommand2 : NetTpCommandMessage
    {
        public string Message { get; set; }
    }
}