using System;
using System.Collections.Generic;
using System.Threading;
using Avdm.Core.Di;
using Avdm.NetTp.Core;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;
using Xunit;
using Moq;
using StructureMap;

namespace Avdm.NetTp.UnitTests.Grid
{
    
    public class SupervisionTreeTests
    {
        [Fact]
        public void SupervisorTreeOneForOne()
        {
            //Supervisor tree
            // [A]----> [B]----> [C]

            var clock = new Mock<IClock>();
            clock.Setup( c => c.Now ).Returns( DateTime.Now );

            ObjectFactory.Configure( c =>
            {
                c.For<IClock>().Singleton().Use( clock.Object );
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );


            Mock<IExecutor> executorCMock = null;

            Action<Node, CancellationToken> work = ( n, c ) => c.WaitHandle.WaitOne();
            var log = new List<string>();
            int count = 0;

            Func<Node> createNodeB = () =>
                {
                    count++;
                    log.Add( "start - B" + count );

                    var nodeB = new Node( "tests", "B", NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.Permanent( 2, TimeSpan.FromMinutes( 20 ) ) );
                    nodeB.NodeEnded += ( o, e ) => log.Add( "stop - B" + count );

                    executorCMock = new Mock<IExecutor>();
                    executorCMock.Setup( c => c.Start() ).Callback( () => log.Add( "start - C" + count ) );
                    executorCMock.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => log.Add( "stop - C" + count ) );
                    var executorC = executorCMock.Object;

                    nodeB.Supervise( executorC );

                    return nodeB;
                };

            var nodeA = new Node( "tests", "A", NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.Permanent( 10, TimeSpan.FromMinutes( 20 ) ) );
            nodeA.NodeEnded += ( o, e ) => log.Add( "stop - A" );

            nodeA.Supervise( createNodeB, work );

            Assert.Equal( 2, log.Count );
            Assert.Equal( "start - B1", log[0] );
            Assert.Equal( "start - C1", log[1] );

            executorCMock.Raise( e => e.Exited += null, ExecutorExitedEventArgs.Success );
            Assert.Equal( 3, log.Count );
            Assert.Equal( "start - C1", log[2] );

            executorCMock.Raise( e => e.Exited += null, ExecutorExitedEventArgs.Success );
            Assert.Equal( 4, log.Count );
            Assert.Equal( "start - C1", log[3] );

            executorCMock.Raise( e => e.Exited += null, ExecutorExitedEventArgs.Success );
            Assert.Equal( 8, log.Count );
            Assert.Equal( "stop - C1", log[4] );
            Assert.Equal( "stop - B1", log[5] );
            Assert.Equal( "start - B2", log[6] );
            Assert.Equal( "start - C2", log[7] );

            executorCMock.Raise( e => e.Exited += null, ExecutorExitedEventArgs.Success );
            Assert.Equal( 9, log.Count );
            Assert.Equal( "start - C2", log[8] );

            executorCMock.Raise( e => e.Exited += null, ExecutorExitedEventArgs.Success );
            Assert.Equal( 10, log.Count );
            Assert.Equal( "start - C2", log[9] );

            executorCMock.Raise( e => e.Exited += null, ExecutorExitedEventArgs.Success );
            Assert.Equal( 14, log.Count );
            Assert.Equal( "stop - C2", log[10] );
            Assert.Equal( "stop - B2", log[11] );
            Assert.Equal( "start - B3", log[12] );
            Assert.Equal( "start - C3", log[13] );

            nodeA.ShutDown( true );
        }

        [Fact]
        public void SupervisorTreeOneForAll()
        {
            //Supervisor tree            
            //              +----> [C1]  
            //              |            
            // [A]----> [B]-+            
            //              |            
            //              +----> [C2]  

            var clock = new Mock<IClock>();
            clock.Setup( c => c.Now ).Returns( DateTime.Now );

            ObjectFactory.Configure( c =>
            {
                c.For<IClock>().Singleton().Use( clock.Object );
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );


            Mock<IExecutor> executorCxMock = null;
            Mock<IExecutor> executorCyMock = null;

            Action<Node, CancellationToken> work = ( n, c ) => c.WaitHandle.WaitOne();
            var log = new List<string>();
            int count = 0;

            Func<Node> createNodeB = () =>
            {
                count++;
                log.Add( "start - B" + count );

                var nodeB = new Node( "tests", "B", NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForAll, NodeSupervisionStrategy.Permanent( 2, TimeSpan.FromMinutes( 20 ) ) );
                nodeB.NodeEnded += ( o, e ) => log.Add( "stop - B" + count );

                executorCxMock = new Mock<IExecutor>();
                executorCxMock.Setup( c => c.Start() ).Callback( () => log.Add( "start - Cx" + count ) );
                executorCxMock.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => log.Add( "stop - Cx" + count ) );
                var executorCx = executorCxMock.Object;
                nodeB.Supervise( executorCx );
                
                executorCyMock = new Mock<IExecutor>();
                executorCyMock.Setup( c => c.Start() ).Callback( () => log.Add( "start - Cy" + count ) );
                executorCyMock.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => log.Add( "stop - Cy" + count ) );
                var executorCy = executorCyMock.Object;
                nodeB.Supervise( executorCy );

                return nodeB;
            };

            var nodeA = new Node( "tests", "A", NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.Permanent( 10, TimeSpan.FromMinutes( 20 ) ) );
            nodeA.NodeEnded += ( o, e ) => log.Add( "stop - A" );

            nodeA.Supervise( createNodeB, work );

            Assert.Equal( 3, log.Count );
            Assert.Equal( "start - B1", log[0] );
            Assert.Equal( "start - Cx1", log[1] );
            Assert.Equal( "start - Cy1", log[2] );

            executorCxMock.Raise( e => e.Exited += null, ExecutorExitedEventArgs.Success );
            Assert.Equal( 7, log.Count );
            Assert.Equal( "stop - Cy1", log[3] );
            Assert.Equal( "stop - Cx1", log[4] );
            Assert.Equal( "start - Cx1", log[5] );
            Assert.Equal( "start - Cy1", log[6] );

            executorCyMock.Raise( e => e.Exited += null, ExecutorExitedEventArgs.Success );
            Assert.Equal( 11, log.Count );
            Assert.Equal( "stop - Cy1", log[7] );
            Assert.Equal( "stop - Cx1", log[8] );
            Assert.Equal( "start - Cx1", log[9] );
            Assert.Equal( "start - Cy1", log[10] );

            executorCxMock.Raise( e => e.Exited += null, ExecutorExitedEventArgs.Success );
            Assert.Equal( 17, log.Count );
            Assert.Equal( "stop - Cy1", log[11] );
            Assert.Equal( "stop - Cx1", log[12] );
            Assert.Equal( "stop - B1", log[13] );
            Assert.Equal( "start - B2", log[14] );
            Assert.Equal( "start - Cx2", log[15] );
            Assert.Equal( "start - Cy2", log[16] );

            nodeA.ShutDown( true );
        }
    }
}