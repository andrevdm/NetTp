using System.Collections.Generic;
using Avdm.NetTp.Messaging;

namespace Avdm.NetTp.Grid.Pool
{
    public class SbinFilesUpdatedEventMessage : NetTpEventMessage
    {
        public List<string> FileNames { get; set; }

        public SbinFilesUpdatedEventMessage()
        {
            FileNames = new List<string>();
        }
    }
}