using System;
using Avdm.Core.Console;
using Avdm.NetTp.Core;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;
using Avdm.NetTp.Messaging;
using StructureMap;

namespace Avdm.NetTp.Tester
{
    public class TestOutOfProcRpcA : INodeFactory
    {
        public Node Create( string applicationName, string nodeName )
        {
            var nodeA = new Node(
                applicationName,
                nodeName,
                NodeWorkerStrategy.Supervise,
                NodeRestartStrategy.OneForOne,
                NodeSupervisionStrategy.DefaultImortal );

            nodeA.StartWorker( ( _, cancel ) =>
                {
                    var bus = ObjectFactory.GetInstance<INetTpMessageBus>();

                    while( true )
                    {
                        ConsoleAsync.WriteLine( ConsoleColor.DarkYellow, "Press any key to send RPC" );
                        Console.ReadKey( true );
                        ConsoleAsync.WriteLine( ConsoleColor.DarkYellow, "NodeA calling RPC" );
                        var response = bus.Rpc<DummyRpcRequestMessage, DummyRpcResponseMessage>( new DummyRpcRequestMessage( DateTime.Now.ToString() ) );
                        ConsoleAsync.WriteLine( ConsoleColor.DarkYellow, "Got response {0}", response.ResponseMessage );
                    }
                } );

            var psi = Node.CreateNodeProcessStartInfo<TestOutOfProcRpcB>( applicationName, "TestOutOfProcRpcA.TestOutOfProcRpcA" );
            nodeA.Supervise( new ProcessExecutor( "TestOutOfProcRpcA.exec1", null, null, psi, null ) );

            return nodeA;
        }
    }
}