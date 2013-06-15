using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;

namespace Avdm.NetTp.Tester
{
    public class TestSupervisionPairB : INodeFactory
    {
        public Node Create( string applicationName, string nodeName )
        {
            var nodeB = new Node(
                applicationName, 
                nodeName,
                NodeWorkerStrategy.DontSupervise,
                NodeRestartStrategy.OneForOne,
                NodeSupervisionStrategy.DefaultPermanent );

            var path = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            path = Path.Combine( path, "NetTp.AppRunnerStub.exe" );
            var psi = Node.CreateNodeProcessStartInfo<TestSupervisionPairA>( applicationName, "TestSupervisionPairB.TestSupervisionPairA" );

            nodeB.Supervise( new ProcessExecutor(
                                 applicationName + ":TestSupervisionPairB.exec1",
                                 null,
                                 null,
                                 psi,
                                 () => Process.GetProcesses().FirstOrDefault( process => process.MainWindowTitle.StartsWith( "NetTp.Tester.TestSupervisionPairA, NetTp.Tester" ) ) ) );

            return nodeB;
        }
    }
}