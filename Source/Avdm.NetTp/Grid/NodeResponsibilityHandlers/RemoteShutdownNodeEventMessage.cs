using System;
using Avdm.Core;
using Avdm.NetTp.Core;
using Avdm.NetTp.Messaging;

namespace Avdm.NetTp.Grid.NodeResponsibilityHandlers
{
    [Serializable]
    public class RemoteShutdownNodeEventMessage : NetTpEventMessage
    {
        public bool KillEverything { get; set; }
        public Guid NodeId { get; set; }
        public string ApplicationName { get; set; }
        public string NodeName { get; set; }
        public bool ReportSuccess { get; set; }

        public RemoteShutdownNodeEventMessage()
        {
            NodeId = Guid.Empty;
            ExpireAt = DateTime.Now.AddMinutes( 2 );
        }

        public RemoteShutdownNodeEventMessage( Guid nodeId )
            : this( nodeId, false )
        {
        }

        public RemoteShutdownNodeEventMessage( Guid nodeId, bool reportSuccess )
        {
            NodeId = nodeId;
            ReportSuccess = reportSuccess;
        }

        public RemoteShutdownNodeEventMessage( string applicationName, string nodeName, bool reportSuccess )
        {
            Preconditions.CheckNotNull( applicationName, "applicationName" );
            Preconditions.CheckNotNull( nodeName, "nodeName" );

            ApplicationName = applicationName;
            NodeName = nodeName;

            ReportSuccess = reportSuccess;
        }
    }
}