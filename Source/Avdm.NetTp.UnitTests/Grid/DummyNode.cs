using System;
using System.Threading;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;

namespace Avdm.NetTp.UnitTests.Grid
{
    internal class DummyNode : Node, IDisposable
    {
        public ManualResetEvent WaitForWorkerToStart { get; private set; }
        public ManualResetEvent WaitForWorkerToEnd { get; private set; }

        public DummyNode( string applicationName, string nodeName, NodeWorkerStrategy nodeWorkerStrategy, NodeRestartStrategy restartStrategy, NodeSupervisionStrategy supervisionStrategy )
            : base( applicationName, nodeName, nodeWorkerStrategy, restartStrategy, supervisionStrategy )
        {
            WaitForWorkerToStart = new ManualResetEvent( false );
            WaitForWorkerToEnd = new ManualResetEvent( false );

            WorkerStarted += ( o, e ) =>
                {
                    WaitForWorkerToStart.Set();
                    WaitForWorkerToEnd.Reset();
                };

            WorkerEnded += ( o, e ) =>
                {
                    WaitForWorkerToStart.Reset();
                    WaitForWorkerToEnd.Set();
                };
        }

        public override void Dispose()
        {
            WaitForWorkerToStart.Close();
            WaitForWorkerToEnd.Close();
            base.Dispose();
        }
    }
}