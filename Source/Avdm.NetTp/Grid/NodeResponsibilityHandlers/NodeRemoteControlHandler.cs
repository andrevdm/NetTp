using System;
using Avdm.Core;
using Avdm.Core.Patterns;
using Avdm.NetTp.Core;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Messaging;
using StructureMap;

namespace Avdm.NetTp.Grid.NodeResponsibilityHandlers
{
    public class NodeRemoteControlHandler: IChainOfResponsibilityHandler<Node,object>
    {
        private readonly Node m_client;
        private readonly INetTpMessageBus m_bus;

        public NodeRemoteControlHandler( Node client )
        {
            Preconditions.CheckNotNull( client, "client" );

            m_bus = ObjectFactory.GetInstance<INetTpMessageBus>();
            m_client = client;

            m_bus.SubscribeToEvent<RemoteShutdownNodeEventMessage>( m_client.NodeName, HandleRemoteShutdown );
        }

        private void HandleRemoteShutdown( RemoteShutdownNodeEventMessage message )
        {
            bool kill = message.KillEverything;

            if( !kill && (message.NodeId != Guid.Empty) )
            {
                kill = message.NodeId == m_client.Id;
            }

            if( !kill && (message.NodeId == Guid.Empty) )
            {
                kill = (message.ApplicationName == m_client.ApplicationName) && (message.NodeName == m_client.NodeName);
            }

            if( kill )
            {
                Console.WriteLine( "HandleRemoteShutdown. Shutting down node" );
                m_client.ShutDown( message.ReportSuccess );
            }
        }

        public bool Handle( Node client, object data, bool wasHandled )
        {
            return false;
        }
    }
}
