using System;
using Avdm.NetTp.Messaging;

namespace Avdm.NetTp.Grid.Nodes
{
    public class NodeLoggingEventMessage : NetTpEventMessage
    {
        public NodeLogging EventType { get; set; }
        public string EventTypeString { get; set; }
        public Guid NodeId { get; set; }
        public string NodeDescription { get; set; }
        public string Description { get; set; }
        public string Exception { get; set; }

        public static NodeLoggingEventMessage ProcessFailed( Node node, Exception exception, string description )
        {
            return new NodeLoggingEventMessage
                {
                    NodeId = node != null ? node.Id : Guid.Empty,
                    NodeDescription = node != null ? node.NodeName : null,
                    Description =  description,
                    Exception = exception.ToString(),
                    EventType = NodeLogging.ProcessFailed,
                    EventTypeString = NodeLogging.ProcessFailed.ToString()
                };
        }

        public static NodeLoggingEventMessage ProcessSucceeded( Node node, string description )
        {
            return new NodeLoggingEventMessage
            {
                NodeId = node != null ? node.Id : Guid.Empty,
                NodeDescription = node != null ? node.NodeName : null,
                Description = description,
                Exception = null,
                EventType = NodeLogging.ProcessSucceeded,
                EventTypeString = NodeLogging.ProcessSucceeded.ToString()
            };
        }

        public static NodeLoggingEventMessage Error( Node node, string description )
        {
            return Error( node, null, description );
        }

        public static NodeLoggingEventMessage Error( Node node, object exceptionObject, string description )
        {
            return new NodeLoggingEventMessage
            {
                NodeId = node != null ? node.Id : Guid.Empty,
                NodeDescription = node != null ? node.NodeName : null,
                Description = description,
                Exception = exceptionObject != null ? exceptionObject.ToString() : null,
                EventType = NodeLogging.Error,
                EventTypeString = NodeLogging.Error.ToString()
            };
        }

        public static NodeLoggingEventMessage Started( Node node, string description )
        {
            return new NodeLoggingEventMessage
            {
                NodeId = node != null ? node.Id : Guid.Empty,
                NodeDescription = node != null ? node.NodeName : null,
                Description = description,
                Exception = null,
                EventType = NodeLogging.Started,
                EventTypeString = NodeLogging.Started.ToString()
            };
        }

        public static NodeLoggingEventMessage ShutDown( Node node, string description )
        {
            return new NodeLoggingEventMessage
            {
                NodeId = node != null ? node.Id : Guid.Empty,
                NodeDescription = node != null ? node.NodeName : null,
                Description = description,
                Exception = null,
                EventType = NodeLogging.ShutDown,
                EventTypeString = NodeLogging.ShutDown.ToString()
            };
        }

        public static NodeLoggingEventMessage Supervising( Node node, string description )
        {
            return new NodeLoggingEventMessage
            {
                NodeId = node != null ? node.Id : Guid.Empty,
                NodeDescription = node != null ? node.NodeName : null,
                Description = description,
                Exception = null,
                EventType = NodeLogging.Supervising,
                EventTypeString = NodeLogging.Supervising.ToString()
            };
        }

        public static NodeLoggingEventMessage ChildClosed( Node node, string description )
        {
            return new NodeLoggingEventMessage
            {
                NodeId = node != null ? node.Id : Guid.Empty,
                NodeDescription = node != null ? node.NodeName : null,
                Description = description,
                Exception = null,
                EventType = NodeLogging.ChildClosed,
                EventTypeString = NodeLogging.ChildClosed.ToString()
            };
        }

        public static NodeLoggingEventMessage ChildFailed( Node node, string description )
        {
            return new NodeLoggingEventMessage
            {
                NodeId = node != null ? node.Id : Guid.Empty,
                NodeDescription = node != null ? node.NodeName : null,
                Description = description,
                Exception = null,
                EventType = NodeLogging.ChildFailed,
                EventTypeString = NodeLogging.ChildFailed.ToString()
            };
        }

        public static NodeLoggingEventMessage ChildRestarted( Node node, string description )
        {
            return new NodeLoggingEventMessage
            {
                NodeId = node != null ? node.Id : Guid.Empty,
                NodeDescription = node != null ? node.NodeName : null,
                Description = description,
                Exception = null,
                EventType = NodeLogging.ChildRestarted,
                EventTypeString = NodeLogging.ChildRestarted.ToString()
            };
        }

        public static NodeLoggingEventMessage WorkerEnded( Node node, string description )
        {
            return new NodeLoggingEventMessage
            {
                NodeId = node != null ? node.Id : Guid.Empty,
                NodeDescription = node != null ? node.NodeName : null,
                Description = description,
                Exception = null,
                EventType = NodeLogging.WorkerEnded,
                EventTypeString = NodeLogging.WorkerEnded.ToString()
            };
        }
    }
}