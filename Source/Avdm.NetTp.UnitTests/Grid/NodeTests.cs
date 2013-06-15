using System.Collections.Generic;
using System.Threading;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;
using Xunit;
using Moq;
using StructureMap;

namespace Avdm.NetTp.UnitTests.Grid
{
    
    public class NodeTests
    {
        [Fact]
        public void StartAction()
        {
            ObjectFactory.Configure( c =>
            {
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );

            using( var startWait = new ManualResetEvent( false ) )
            using( var runWait = new ManualResetEvent( false ) )
            {
                var node = new DummyNode( "tests", "test", NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultImortal );

                Assert.False( node.WorkerExecuting, "Node has not yet started" );

                node.StartWorker( ( n, c ) =>
                    {
                        startWait.Set();
                        runWait.WaitOne();
                    } );

                startWait.WaitOne( 2000 );
                Assert.True( node.WorkerExecuting, "Node has started" );
            }               
        }

        [Fact]
        public void ShutdownCancels()
        {
            ObjectFactory.Configure( c =>
            {
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );

            using( var startWait = new ManualResetEvent( false ) )
            using( var exitedWait = new ManualResetEvent( false ) )
            {
                bool finished = false;

                var node = new DummyNode( "tests", "test", NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultImortal );

                Assert.False( node.WorkerExecuting, "Node has not yet started" );

                node.StartWorker( ( n, token ) =>
                {
                    startWait.Set();
                    token.WaitHandle.WaitOne();
                    finished = true;
                    exitedWait.Set();
                } );

                startWait.WaitOne( 2000 );
                Assert.True( node.WorkerExecuting, "Node has started" );
                
                node.ShutDown( false );
                exitedWait.WaitOne( 2000 );
                Assert.False( node.WorkerExecuting, "Node has been stopped - worker should have been cancelled" );

                Assert.True( finished, "Should have finished" );
            }
        }

        [Fact]
        public void StopOnParentStopsAllChildren()
        {
            ObjectFactory.Configure( c =>
            {
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );

            var node = new Node( "tests", "test", NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultImortal );

            var executor1 = new Mock<IExecutor>();
            var executor2 = new Mock<IExecutor>();
            var executor3 = new Mock<IExecutor>();

            node.Supervise( executor1.Object );
            node.Supervise( executor2.Object );
            node.Supervise( executor3.Object );

            node.ShutDown( true );

            executor1.Verify( e => e.ShutDown( It.IsAny<bool>() ), Times.Once(), "Executor1 should have been stopped" );
            executor2.Verify( e => e.ShutDown( It.IsAny<bool>() ), Times.Once(), "Executor2 should have been stopped" );
            executor3.Verify( e => e.ShutDown( It.IsAny<bool>() ), Times.Once(), "Executor3 should have been stopped" );
        }

        [Fact]
        public void ChildrenStoppedInReverseOrderOfShutdown()
        {
            ObjectFactory.Configure( c =>
            {
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );

            var order = new List<int>();

            var executor1 = new Mock<IExecutor>();
            executor1.Setup( e => e.ShutDown( It.IsAny<bool>() ) ).Callback( () => order.Add( 1 ) );

            var executor2 = new Mock<IExecutor>();
            executor2.Setup( e => e.ShutDown( It.IsAny<bool>() ) ).Callback( () => order.Add( 2 ) );

            var executor3 = new Mock<IExecutor>();
            executor3.Setup( e => e.ShutDown( It.IsAny<bool>() ) ).Callback( () => order.Add( 3 ) );

            var node = new Node( "tests", "test", NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultImortal );

            node.Supervise( executor1.Object );
            node.Supervise( executor2.Object );
            node.Supervise( executor3.Object );

            executor1.Verify( e => e.Start(), Times.Exactly( 1 ), "Should be auto-started" );
            executor2.Verify( e => e.Start(), Times.Exactly( 1 ), "Should be auto-started" );
            executor3.Verify( e => e.Start(), Times.Exactly( 1 ), "Should be auto-started" );


            node.ShutDown( true );

            executor1.Verify( e => e.Start(), Times.Exactly( 1 ), "Not expecting a restart" );
            executor2.Verify( e => e.Start(), Times.Exactly( 1 ), "Not expecting a restart" );
            executor3.Verify( e => e.Start(), Times.Exactly( 1 ), "Not expecting a restart" );

            Assert.Equal( 3, order.Count ); //"All three children should have been stopped"

            Assert.Equal( 3, order[0] ); //"child 3 should have been shutdown 1st"
            Assert.Equal( 2, order[1] ); //"child 2 should have been shutdown 2nd"
            Assert.Equal( 1, order[2] ); //"child 1 should have been shutdown 3rd"
        }
    }
}
