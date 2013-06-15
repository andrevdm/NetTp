using System;
using System.Diagnostics;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;

namespace Avdm.NetTp.Tester
{
    public class TestRunnableNodeOutOfProcNodeB : INodeFactory
    {
        public Node Create( string applicationName, string nodeName )
        {
            var nodeB = new Node(
                applicationName, 
                nodeName, 
                NodeWorkerStrategy.DontSupervise,
                NodeRestartStrategy.OneForAll,
                NodeSupervisionStrategy.Permanent( 3, TimeSpan.FromMinutes( 2 ), new[] { 0, 200, 500 } ) );

            var psi = new ProcessStartInfo { FileName = @"C:\Windows\System32\notepad.exe" };
            var executorB1 = new ProcessExecutor( applicationName + ":nodeB.execB1", null, null, psi, null );
            executorB1.Exited += ( o, e ) => Console.WriteLine( "notepad executor exited - node B, succeeded={0}", e.Succeeded );
            nodeB.Supervise( executorB1 );

            psi = new ProcessStartInfo { FileName = @"C:\Windows\System32\winver.exe" };
            var executorB2 = new ProcessExecutor( applicationName + ":nodeB.execB2", null, null, psi, null );
            executorB2.Exited += ( o, e ) => Console.WriteLine( "winver executor exited - node B, succeeded={0}", e.Succeeded );
            nodeB.Supervise( executorB2 );

            return nodeB;
        }
    }
}