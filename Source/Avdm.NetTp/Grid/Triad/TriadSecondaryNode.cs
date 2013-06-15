using System;
using Avdm.Core;
using Avdm.Core.Logging;
using Avdm.NetTp.Core;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;
using StructureMap;

namespace Avdm.NetTp.Grid.Triad
{
    /// <summary>
    /// The triad secondary supervisors the triad primary
    /// 
    /// The triad consists of the triad primary, triad secondary and the application node
    /// Each runs in its own process. The supervision tree looks like this
    /// 
    ///          +-------------------+           
    ///          |                   |           
    ///          V                   |           
    ///    [triad primary]           |           
    ///          |            [triad secondary]  
    ///          |                   ^           
    ///          V                   |           
    ///   [application node]---------+           
    ///          |                               
    ///          V                               
    /// 
    /// This triad of node (processes) ensures that the process graph is kept in a robust a fashion as possible
    /// </summary>
    public class TriadSecondaryNode : INodeFactory
    {
        public Node Create( string applicationName, string nodeName )
        {
            try
            {
                Preconditions.CheckNotBlank( applicationName, "applicationName" );

                var nodeFinder = ObjectFactory.GetInstance<INodeFinder>();

                if( nodeFinder.FindNodeProcessByNodeName( applicationName, nodeName ) != null )
                {
                    throw new InvalidOperationException( string.Format( "Node is already running. App={0}, nodeName={1}", applicationName, nodeName ) );
                }

                var secondaryNode = new Node( 
                    applicationName,
                    "TriadSecondaryNode", 
                    NodeWorkerStrategy.DontSupervise, 
                    NodeRestartStrategy.OneForOne, 
                    NodeSupervisionStrategy.DefaultPermanent );

                secondaryNode.Supervise(
                    new NodeProcessExecutor<TriadPrimaryNode>(
                        applicationName,
                        "TriadPrimaryNode",
                        null,
                        null ) );

                return secondaryNode;
            }
            catch( Exception ex )
            {
                Log.Error( string.Format( "TriadSecondaryNode: {0}, {1}", applicationName, nodeName ), ex );
                throw;
            }
        }
    }
}