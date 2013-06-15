using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;
using Xunit;

namespace Avdm.NetTp.UnitTests.Grid
{
    public class WorkerSupervisionTests
    {
        [Fact]
        public void DontSuperviseDoesNotShutdown()
        {
            bool shutdown = false;

            var node = new Node( "tests", "test", NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultTemporary );
            node.NodeEnded += ( o, e ) => shutdown = true;
            node.StartWorker( ( n, c ) => { } );

            Assert.False( shutdown, "The worker is not being supervised. No shutdown is expected" );
        }

        [Fact]
        public void SuperviseDoesShutdown()
        {
            bool shutdown = false;

            var node = new Node( "tests", "test", NodeWorkerStrategy.Supervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultTemporary );
            node.NodeEnded += ( o, e ) => shutdown = true;
            node.StartWorker( ( n, c ) => { } );

            Assert.False( shutdown, "The worker is being supervised. Shutdown is expected" );
        }
    }
}