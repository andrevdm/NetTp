using System;
using System.Diagnostics;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;

namespace Avdm.NetTp.Tester
{
    public class TestRunnableNodeOutOfProcNodeA : INodeFactory
    {
        public Node Create( string applicationName, string nodeName )
        {
            var nodeA = new Node( applicationName, nodeName, NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultImortal );
            nodeA.StartWorker( ( n, c ) => { } );

            var psiB = Node.CreateNodeProcessStartInfo<TestRunnableNodeOutOfProcNodeB>( applicationName, "TestRunnableNodeOutOfProcNodeA.nodeB" );
            var psiC = Node.CreateNodeProcessStartInfo<TestRunnableNodeOutOfProcNodeC>( applicationName, "TestRunnableNodeOutOfProcNodeA.nodeC" );

            var psi = new ProcessStartInfo { FileName = @"C:\Windows\System32\calc.exe" };
            var executorA1 = new ProcessExecutor( "TestApp:execA1", null, null, psi, null );
            executorA1.Exited += ( o, e ) => Console.WriteLine( "calc exited, succeeded={0}", e.Succeeded );

            nodeA.Supervise( new ProcessExecutor( "TestApp:nodeA.exec1", null, null, psiB, null ) );
            nodeA.Supervise( new ProcessExecutor( "TestApp:nodeA.exec2", null, null, psiC, null ) );
            nodeA.Supervise( executorA1 );

            return nodeA;
        }
    }
}