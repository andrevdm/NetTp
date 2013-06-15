using System;
using Avdm.Core;
using Avdm.NetTp.Core;
using Avdm.NetTp.Grid.Config;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;
using StructureMap;

namespace Avdm.NetTp.Grid.Triad
{
    /// <summary>
    /// The application node is supervised by the triad primary
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
    public class ApplicationNode : INodeFactory
    {
        public Node Create( string applicationName, string nodeName )
        {
            try
            {
                Preconditions.CheckNotBlank( applicationName, "applicationName" );

                var nodeFinder = ObjectFactory.GetInstance<INodeFinder>();

                if( nodeFinder.FindNodeProcessByNodeName( applicationName, "ApplicationNode" ) != null )
                {
                    throw new InvalidOperationException( string.Format( "Node is already running. App={0}, nodeName={1}", applicationName, "ApplicationNode" ) );
                }

                var configLoader = ObjectFactory.GetInstance<INodeConfigPersistor>();
                var config = configLoader.LoadAppConfig( applicationName );

                if( config == null )
                {
                    throw new InvalidOperationException( string.Format( "No config found for application {0}", applicationName ) );
                }

                config.Name = "ApplicationNode";

                var applicationNode = new Node(
                    applicationName,
                    config,
                    node =>
                    {
                        //Create the triad secondary in the preInit so that it will be created before the other nodes
                        // this means that if rest-for-one is used it wont be killed when the other children are killed
                        node.Supervise(
                            new NodeProcessExecutor<TriadSecondaryNode>(
                                applicationName,
                                "TriadSecondaryNode",
                                null,
                                null ) );

                    } );

                return applicationNode;
            }
            catch( Exception ex )
            {
                Console.WriteLine( ex );
                throw;
            }
        }
    }
}
