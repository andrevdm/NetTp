using System;
using System.Collections.Generic;
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
    public class SupervisionTests
    {
        [Fact]
        public void TemporarySupervisionNeverRestarts()
        {
            ObjectFactory.Configure( c =>
            {
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );

            var node = new Node( "tests", "test", NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultTemporary );

            var executor1 = new Mock<IExecutor>();
            node.Supervise( executor1.Object );
            executor1.Verify( e => e.Start(), Times.Exactly( 1 ), "autostart" );

            //Simulate the executor stopping executing
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( true ) );

            executor1.Verify( e => e.Start(), Times.Exactly( 1 ), "The executor should not been restarted" );
        }

        [Fact]
        public void ImotalSupervisionRestarts()
        {
            var clock = new Mock<IClock>();

            ObjectFactory.Configure( c =>
            {
                c.For<IClock>().Singleton().Use( clock.Object );
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );


            var node = new Node( "tests", "test", NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultImortal );

            var executor1 = new Mock<IExecutor>();
            node.Supervise( executor1.Object );
            executor1.Verify( e => e.Start(), Times.Exactly( 1 ), "autostart" );

            //Simulate the executor stopping executing
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( true ) );

            executor1.Verify( e => e.Start(), Times.Exactly( 2 ), "The executor should have been restarted" );
        }

        [Fact]
        public void ImotalSupervisionRestartsWithDelay()
        {
            var waited = new List<int>();

            var clock = new Mock<IClock>();
            clock.Setup( c => c.Now ).Returns( DateTime.Now );
            clock.Setup( c => c.ThreadSleep( It.IsAny<int>() ) ).Callback<int>( waited.Add );
            clock.Setup( c => c.ThreadSleep( It.IsAny<TimeSpan>() ) ).Callback<TimeSpan>( i => waited.Add( (int)i.TotalMilliseconds ) );

            ObjectFactory.Configure( c =>
            {
                c.For<IClock>().Singleton().Use( clock.Object );
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );


            var node = new Node(
                "tests", 
                "test",
                NodeWorkerStrategy.DontSupervise,
                NodeRestartStrategy.OneForOne,
                NodeSupervisionStrategy.Imortal( TimeSpan.FromMinutes( 1 ), new[] {1, 2, 3, 4} ) );

            var executor1 = new Mock<IExecutor>();
            node.Supervise( executor1.Object );
            executor1.Verify( e => e.Start(), Times.Exactly( 1 ), "autostart" );
            Assert.Equal( 0, waited.Count );//"No waits expected yet"

            //Simulate the executor stopping executing
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( true ) );
            executor1.Verify( e => e.Start(), Times.Exactly( 2 ), "The executor should have been restarted" );
            Assert.Equal( 1, waited.Count );//"Restart should have been delayed"
            Assert.Equal( 1, waited[0] );//"Restart should have been delayed"
            
            //Simulate the executor stopping executing
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( true ) );
            executor1.Verify( e => e.Start(), Times.Exactly( 3 ), "The executor should have been restarted" );
            Assert.Equal( 2, waited.Count );//"Restart should have been delayed"
            Assert.Equal( 2, waited[1] );//"Restart should have been delayed"
        }

        [Fact]
        public void ImotalSupervisionRestartsWithZeroDelay()
        {
            var waited = new List<int>();

            var clock = new Mock<IClock>();
            clock.Setup( c => c.Now ).Returns( DateTime.Now );
            clock.Setup( c => c.ThreadSleep( It.IsAny<int>() ) ).Callback<int>( waited.Add );
            clock.Setup( c => c.ThreadSleep( It.IsAny<TimeSpan>() ) ).Callback<TimeSpan>( i => waited.Add( (int)i.TotalMilliseconds ) );

            ObjectFactory.Configure( c =>
            {
                c.For<IClock>().Singleton().Use( clock.Object );
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );


            var node = new Node(
                "tests", 
                "test",
                NodeWorkerStrategy.DontSupervise,
                NodeRestartStrategy.OneForOne,
                NodeSupervisionStrategy.Imortal( TimeSpan.FromMinutes( 1 ), new[] { 0 } ) );

            var executor1 = new Mock<IExecutor>();
            node.Supervise( executor1.Object );
            executor1.Verify( e => e.Start(), Times.Exactly( 1 ), "autostart" );
            Assert.Equal( 0, waited.Count );//"No waits expected"

            //Simulate the executor stopping executing
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( true ) );
            executor1.Verify( e => e.Start(), Times.Exactly( 2 ), "The executor should have been restarted" );
            Assert.Equal( 0, waited.Count );//"No waits expected"

            //Simulate the executor stopping executing
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( true ) );
            executor1.Verify( e => e.Start(), Times.Exactly( 3 ), "The executor should have been restarted" );
            Assert.Equal( 0, waited.Count );//"No waits expected"
        }

        [Fact]
        public void TransientSupervisionRestartsOnAbnormalExit()
        {
            ObjectFactory.Configure( c =>
            {
                c.For<IClock>().Use( x => new Mock<IClock>().Object );
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );

            var node = new Node( "tests", "test", NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultTransient );

            var executor1 = new Mock<IExecutor>();
            node.Supervise( executor1.Object );
            executor1.Verify( e => e.Start(), Times.Exactly( 1 ), "autostart" );

            //Simulate the executor stopping executing
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( false ) );

            executor1.Verify( e => e.Start(), Times.Exactly( 2 ), "The executor should have been restarted" );
        }

        [Fact]
        public void TransientSupervisionDoesNotRestartsOnNormalExit()
        {
            ObjectFactory.Configure( c =>
            {
                c.For<IClock>().Use( x => new Mock<IClock>().Object );
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );

            var node = new Node( "tests", "test", NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultTransient );

            var executor1 = new Mock<IExecutor>();
            node.Supervise( executor1.Object );
            executor1.Verify( e => e.Start(), Times.Exactly( 1 ), "autostart" );

            //Simulate the executor stopping executing
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( true ) );

            executor1.Verify( e => e.Start(), Times.Exactly( 1 ), "The executor should not have been restarted" );
        }

        [Fact]
        public void TransientSupervisionRestartsOnAbnormalExitWithDelay()
        {
            var waited = new List<int>();

            var clock = new Mock<IClock>();
            clock.Setup( c => c.Now ).Returns( DateTime.Now );
            clock.Setup( c => c.ThreadSleep( It.IsAny<int>() ) ).Callback<int>( waited.Add );
            clock.Setup( c => c.ThreadSleep( It.IsAny<TimeSpan>() ) ).Callback<TimeSpan>( i => waited.Add( (int)i.TotalMilliseconds ) );

            ObjectFactory.Configure( c =>
            {
                c.For<IClock>().Singleton().Use( clock.Object );
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );

            var node = new Node(
                "tests", 
                "test",
                NodeWorkerStrategy.DontSupervise,
                NodeRestartStrategy.OneForOne,
                NodeSupervisionStrategy.Transient( 2, TimeSpan.FromMinutes( 1 ), new[] { 1, 2, 3, 4 } ) );

            var executor1 = new Mock<IExecutor>();
            node.Supervise( executor1.Object );
            executor1.Verify( e => e.Start(), Times.Exactly( 1 ), "autostart" );
            Assert.Equal( 0, waited.Count );//"No waits expected yet"

            //Simulate the executor stopping executing
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( false ) );
            executor1.Verify( e => e.Start(), Times.Exactly( 2 ), "The executor should have been restarted" );
            Assert.Equal( 1, waited.Count );//"Restart should have been delayed"
            Assert.Equal( 1, waited[0] );//"Restart should have been delayed"

            //Simulate the executor stopping executing
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( false ) );
            executor1.Verify( e => e.Start(), Times.Exactly( 3 ), "The executor should have been restarted" );
            Assert.Equal( 2, waited.Count );//"Restart should have been delayed"
            Assert.Equal( 2, waited[1] );//"Restart should have been delayed"
            
            //Simulate the executor stopping executing
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( false ) );
            executor1.Verify( e => e.ShutDown( false ), Times.Once(), "Failed too many times, expecting shutdown" );
        }

        [Fact]
        public void PermanentSupervisionRestarts()
        {
            var clock = new Mock<IClock>();
            clock.Setup( c => c.Now ).Returns( DateTime.Now );

            ObjectFactory.Configure( c =>
            {
                c.For<IClock>().Singleton().Use( clock.Object );
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );

            var waits = new int[] { 0, 0 };

            var node = new Node(
                "tests", 
                "test", 
                NodeWorkerStrategy.DontSupervise, 
                NodeRestartStrategy.OneForOne, 
                NodeSupervisionStrategy.Permanent( 
                    0, 
                    TimeSpan.MaxValue, 
                    waits ) );

            var executor1 = new Mock<IExecutor>();
            node.Supervise( executor1.Object );
            executor1.Verify( e => e.Start(), Times.Exactly( 1 ), "autostart" );

            //Simulate the executor stopping executing
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( true ) );
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( true ) );

            executor1.Verify( e => e.Start(), Times.Exactly( 3 ), "The executor should have been restarted" );
        }

        [Fact]
        public void PermanentSupervisionWaitsBetweenRestarts()
        {
            var waited = new List<int>();
            var clock = new Mock<IClock>();
            clock.Setup( c => c.Now ).Returns( DateTime.Now );
            clock.Setup( c => c.ThreadSleep( It.IsAny<int>() ) ).Callback<int>( waited.Add );
            clock.Setup( c => c.ThreadSleep( It.IsAny<TimeSpan>() ) ).Callback<TimeSpan>( i => waited.Add( (int)i.TotalMilliseconds ) );

            ObjectFactory.Configure( c =>
            {
                c.For<IClock>().Singleton().Use( clock.Object );
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );

            var waits = new[] { 0, 100 };

            var node = new Node(
                "tests", 
                "test", 
                NodeWorkerStrategy.DontSupervise, 
                NodeRestartStrategy.OneForOne, 
                NodeSupervisionStrategy.Permanent(
                    0,
                    TimeSpan.MaxValue,
                    waits ) );

            var executor1 = new Mock<IExecutor>();
            node.Supervise( executor1.Object );
            executor1.Verify( e => e.Start(), Times.Exactly( 1 ), "autostart" );

            //Simulate the executor stopping executing
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( true ) );
            Assert.Equal( 0, waited.Count );//"First wait is 0ms so there should be not wait"

            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( true ) );
            Assert.Equal( 1, waited.Count );//"Second restarted should have been delayed"
            Assert.Equal( 100, waited[0] );//"Second restarted should have been delayed for 100ms"

            executor1.Verify( e => e.Start(), Times.Exactly( 3 ), "The executor should have been restarted" );
        }

        [Fact]
        public void PermanentSupervisionWaitsBetweenRestartsButResetsAfterMaxTime()
        {
            var waited = new List<int>();
            var clock = new Mock<IClock>();
            clock.Setup( c => c.Now ).Returns( DateTime.Now );
            clock.Setup( c => c.ThreadSleep( It.IsAny<int>() ) ).Callback<int>( waited.Add );
            clock.Setup( c => c.ThreadSleep( It.IsAny<TimeSpan>() ) ).Callback<TimeSpan>( i => waited.Add( (int)i.TotalMilliseconds ) );

            ObjectFactory.Configure( c =>
            {
                c.For<IClock>().Singleton().Use( clock.Object );
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );

            var waits = new[] { 0, 100 };

            var node = new Node(
                "tests", 
                "test", 
                NodeWorkerStrategy.DontSupervise, 
                NodeRestartStrategy.OneForOne, 
                NodeSupervisionStrategy.Permanent(
                    0,
                    TimeSpan.FromMinutes( 1 ),
                    waits ) );

            var executor1 = new Mock<IExecutor>();
            node.Supervise( executor1.Object );
            executor1.Verify( e => e.Start(), Times.Exactly( 1 ), "autostart" );

            //Simulate the executor stopping executing
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( true ) );
            Assert.Equal( 0, waited.Count );//"First wait is 0ms so there should be not wait"


            //Simulate second stop, 5 minutes later
            clock.Setup( c => c.Now ).Returns( DateTime.Now.AddMinutes( 5 )  );
            executor1.Raise( e => e.Exited += null, new ExecutorExitedEventArgs( true ) );

            Assert.Equal( 0, waited.Count );//"Second restarted should not have been delayed"

            executor1.Verify( e => e.Start(), Times.Exactly( 3 ), "The executor should have been restarted" );
        }

        [Fact]
        public void PermanentSupervisionFailsIfTooManyRestartsWithinMaxTime()
        {
            var clock = new Mock<IClock>();

            ObjectFactory.Configure( c =>
            {
                c.For<IClock>().Singleton().Use( clock.Object );
                c.For<INodeResponsabilityProvider>().Use( x => new Mock<INodeResponsabilityProvider>().Object );
            } );

            var waits = new[] { 0, 100 };

            var child = new Mock<IExecutor>();

            var perm = new PermanentNodeSupervisionStrategy( 2, TimeSpan.FromMinutes( 5 ), waits );

            var at = DateTime.Now;

            clock.Setup( c => c.Now ).Returns( at );
            Assert.Equal( NodeExitAction.Restart, perm.ChildExited( child.Object, true ) );//"First restart"

            clock.Setup( c => c.Now ).Returns( at.AddMinutes( 3 ) );
            Assert.Equal( NodeExitAction.Restart, perm.ChildExited( child.Object, true ) );//"Restart within window and within max count"

            clock.Setup( c => c.Now ).Returns( at.AddMinutes( 4 ) );
            Assert.Equal( NodeExitAction.Fail, perm.ChildExited( child.Object, true ) );//"Restart within window and over max count"
        }
    }
}