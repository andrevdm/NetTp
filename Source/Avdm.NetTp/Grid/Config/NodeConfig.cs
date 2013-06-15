using System;
using System.Collections.Generic;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;

namespace Avdm.NetTp.Grid.Config
{
    /// <summary>
    /// Stores the configuration for an tree of nodes (application)
    /// </summary>
    [Serializable]
    public class NodeConfig
    {
        /// <summary>
        /// True if this node should be run in a seperate processes.
        /// An application node will always run as a seperate process regargless of this setting
        /// </summary>
        public bool IsProcess { get; set; }
        
        /// <summary>
        /// GUID for each config item. This is by newly created node processes to find their config
        /// </summary>
        public Guid ConfigId { get; set; }
        
        /// <summary>
        /// Node Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A regex that specifies on which machines a node can run.
        /// The Environment.MachineName is tested against this regex to see if it should be run.
        /// If this is null or blank it will run on all machines.
        /// 
        /// Note that if this does not match the current machine, no child nodes will be started regardless of their RunOn property
        /// </summary>
        public string RunOn { get; set; }

        /// <summary>
        /// The full type name of the worker that implements INodeWorker
        /// </summary>
        public string Worker { get; set; }

        /// <summary>
        /// Indicates if the node will monitor its worker
        /// </summary>
        public NodeWorkerStrategy WorkerStrategy { get; set; }

        /// <summary>
        /// The supervision strategy
        /// </summary>
        public NodeSupervisionStrategy SupervisionStrategy { get; set; }

        /// <summary>
        /// The restart strategy
        /// </summary>
        public NodeRestartStrategy RestartStrategy { get; set; }

        /// <summary>
        /// The child nodes supervised by this node
        /// </summary>
        public List<NodeConfig> Nodes { get; set; }

        /// <summary>
        /// The child processes supervised by this node
        /// </summary>
        public List<NodeProcessChildConfig> Processes { get; set; }

        /// <summary>
        /// Additional settings
        /// </summary>
        public Dictionary<string, string> NodeSettings { get; set; }

        public NodeConfig()
        {
            Nodes = new List<NodeConfig>();
            Processes = new List<NodeProcessChildConfig>();
            WorkerStrategy = NodeWorkerStrategy.DontSupervise;
            SupervisionStrategy = NodeSupervisionStrategy.DefaultTemporary;
            RestartStrategy = NodeRestartStrategy.OneForOne;
        }
    }
}
