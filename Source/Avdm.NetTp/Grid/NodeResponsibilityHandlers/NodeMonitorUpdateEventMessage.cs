using System;
using System.Collections.Generic;
using System.IO;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;
using Avdm.NetTp.Messaging;

namespace Avdm.NetTp.Grid.NodeResponsibilityHandlers
{
    [Serializable]
    public class NodeMonitorUpdateEventMessage : NetTpEventMessage
    {
        public string MachineName { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public Guid NodeId { get; set; }
        public string NodeDescription { get; set; }
        public bool WorkerExecuting { get; set; }
        public NodeWorkerStrategy WorkerStrategy { get; set; }
        public NodeSupervisionStrategy SupervisionStrategy { get; set; }
        public NodeRestartStrategy RestartStrategy { get; set; }
        public List<IExecutorInfo> Executors { get; set; }
        public DateTime CreatedDate { get; set; }

        public NodeMonitorUpdateEventMessage()
        {
            CreatedDate = DateTime.Now;
        }

        public NodeMonitorUpdateEventMessage( 
            string machineName, 
            int processId,
            string processName, 
            Guid nodeId, 
            string nodeDescription, 
            bool workerExecuting, 
            NodeWorkerStrategy workerStrategy,
            NodeSupervisionStrategy supervisionStrategy, 
            NodeRestartStrategy restartStrategy, 
            IEnumerable<IExecutorInfo> executors )
        {
            CreatedDate = DateTime.Now;
            MachineName = machineName;
            ProcessId = processId;
            ProcessName = processName;
            NodeId = nodeId;
            NodeDescription = nodeDescription;
            WorkerExecuting = workerExecuting;
            WorkerStrategy = workerStrategy;
            SupervisionStrategy = supervisionStrategy;
            RestartStrategy = restartStrategy;
            Executors = new List<IExecutorInfo>( executors );
        }

        public void Dump( TextWriter writer )
        {
            writer.WriteLine( "Id: {0}", NodeId );
            writer.WriteLine( "    Description: {0}", NodeDescription );
            writer.WriteLine( "    Running on: {0}", MachineName );
            writer.WriteLine( "       In process.Id: {0}", ProcessId );
            writer.WriteLine( "       In process.Name: {0}", ProcessName );
            writer.WriteLine( "    Worker executing: {0}", WorkerExecuting );
            writer.WriteLine( "    Worker strategy: {0}", WorkerExecuting );
            writer.WriteLine( "    Supervision strategy: {0}", SupervisionStrategy );
            writer.WriteLine( "    Restart strategy: {0}", RestartStrategy );

            if( Executors.Count > 0 )
            {
                writer.WriteLine( "    Supervising" );
                Executors.ForEach( e =>
                    {
                        writer.WriteLine( "       Type: {0}, Id: {1}, Child Id: {2}", e.Id, e.Type, e.ChildId ?? "" );
                    } );
            }
            else
            {
                writer.WriteLine( "    Not supervising any executors" );
            }
        }
    }
}