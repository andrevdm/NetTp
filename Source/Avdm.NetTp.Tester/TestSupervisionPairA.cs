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
    public class TestSupervisionPairA : INodeFactory
    {
        public Node Create( string applicationName, string nodeName )
        {
            var nodeA = new Node(
                applicationName,
                nodeName,
                NodeWorkerStrategy.DontSupervise,
                NodeRestartStrategy.OneForOne,
                NodeSupervisionStrategy.DefaultPermanent );

            var path = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            path = Path.Combine( path, "NetTp.AppRunnerStub.exe" );
            var psi = Node.CreateNodeProcessStartInfo<TestSupervisionPairB>( applicationName, "TestSupervisionPairA.TestSupervisionPairB" );

            nodeA.Supervise( new ProcessExecutor(
                                 applicationName + ":TestSupervisionPairA.exec1",
                                 null,
                                 null,
                                 psi,
                                 () => Process.GetProcesses().FirstOrDefault( process => process.MainWindowTitle.StartsWith( "NetTp.Tester.TestSupervisionPairB, NetTp.Tester" ) ) ) );

            return nodeA;
        }
    }
}