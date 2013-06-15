using System;
using System.Diagnostics;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;

namespace Avdm.NetTp.Tester
{
    public class TestRunnableNodeOutOfProcNodeC : INodeFactory
    {
        public Node Create( string applicationName, string nodeName )
        {
            var nodeC = new Node(
                applicationName,
                nodeName, 
                NodeWorkerStrategy.DontSupervise,
                NodeRestartStrategy.OneForAll,
                NodeSupervisionStrategy.DefaultTransient );

            var psi = new ProcessStartInfo { FileName = @"C:\Windows\System32\ftp.exe" };
            var executor = new ProcessExecutor( applicationName + ":nodeC.execC1", null, null, psi, null );
            executor.Exited += ( o, e ) => Console.WriteLine( "ftp executor exited - node B, succeeded={0}", e.Succeeded );
            nodeC.Supervise( executor );

            return nodeC;
        }
    }
}