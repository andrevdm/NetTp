using System;
using Avdm.NetTp.Grid.NodeResponsibilityHandlers;
using Avdm.NetTp.Messaging;
using StructureMap;

namespace Avdm.NetTp.Grid.Nodes
{
    public class NodeRemoteControl : INodeRemoteControl
    {
        public void ShutDown( Guid nodeId, bool success = false )
        {
            var bus = ObjectFactory.GetInstance<INetTpMessageBus>();
            bus.PublishEvent( new RemoteShutdownNodeEventMessage( nodeId, success ) );
        }
        
        public void ShutDownAll( bool success = false )
        {
            var bus = ObjectFactory.GetInstance<INetTpMessageBus>();
            bus.PublishEvent( new RemoteShutdownNodeEventMessage( Guid.Empty, success ) { KillEverything = true} );
        }
    }
}
