using System;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;

namespace Avdm.NetTp.Tester
{
    public class TestRunnableNode : INodeFactory
    {
        public Node Create( string applicationName, string nodeName )
        {
            var node = new Node( applicationName, nodeName, NodeWorkerStrategy.Supervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultTemporary );

            node.StartWorker( ( n, cancel ) =>
                {
                    Console.WriteLine( "Node worker started" );
                    Console.WriteLine( "press return to exit" );
                    Console.ReadLine();
                    Console.WriteLine( "Node worker ending" );
                } );                    

            return node;
        }
    }
}