using Avdm.Core;
using Avdm.Core.Patterns;
using Avdm.NetTp.Core;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Messaging;
using StructureMap;

namespace Avdm.NetTp.Grid.NodeResponsibilityHandlers
{
    public class NodeLoggingHandler: IChainOfResponsibilityHandler<Node,object>
    {
        private readonly Node m_client;
        private readonly INetTpMessageBus m_bus;

        public NodeLoggingHandler( Node client )
        {
            Preconditions.CheckNotNull( client, "client" );

            m_bus = ObjectFactory.GetInstance<INetTpMessageBus>();
            m_client = client;

        }

        public bool Handle( Node client, NodeActions.Started data, bool wasHandled )
        {
            m_bus.PublishEvent( NodeLoggingEventMessage.Started( client, "Started: " + client.NodeName ) );
            return false;
        }

        public bool Handle( Node client, NodeActions.ShutDown data, bool wasHandled )
        {
            m_bus.PublishEvent( NodeLoggingEventMessage.ShutDown( client, "Shutting down: " + client.NodeName ) );
            return false;
        }

        public bool Handle( Node client, NodeActions.Supervising data, bool wasHandled )
        {
            m_bus.PublishEvent( NodeLoggingEventMessage.Supervising( client, "Supervising child: " + data.Child.ChildName ) );
            return false;
        }

        public bool Handle( Node client, NodeActions.ChildClosed data, bool wasHandled )
        {
            m_bus.PublishEvent( NodeLoggingEventMessage.ChildClosed( client, "Child close: " + data.Child.ChildName ) );
            return false;
        }

        public bool Handle( Node client, NodeActions.ChildFailed data, bool wasHandled )
        {
            m_bus.PublishEvent( NodeLoggingEventMessage.ChildFailed( client, "Child failed: " + data.Child.ChildName ) );
            return false;
        }

        public bool Handle( Node client, NodeActions.ChildRestarted data, bool wasHandled )
        {
            m_bus.PublishEvent( NodeLoggingEventMessage.ChildRestarted( client, "Child restarted: " + data.Child.ChildName ) );
            return false;
        }

        public bool Handle( Node client, NodeActions.WorkerEnded data, bool wasHandled )
        {
            m_bus.PublishEvent( NodeLoggingEventMessage.WorkerEnded( client, "Worker ended: " ) );
            return false;
        }


        public bool Handle( Node client, object data, bool wasHandled )
        {
            return false;
        }
    }
}
