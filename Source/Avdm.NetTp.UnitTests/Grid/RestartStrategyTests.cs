using System.Collections.Generic;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.RestartStrategies;
using Xunit;
using Moq;

namespace Avdm.NetTp.UnitTests.Grid
{
    
    public class RestartStrategyTests
    {
        [Fact]
        public void OneForOneOnlyRestartsExitedChild()
        {
            var restarted = new List<int>();

            var child1 = new Mock<IExecutor>();
            child1.Setup( c => c.Start() ).Callback( () => restarted.Add( 1 ) );

            var child2 = new Mock<IExecutor>();
            child2.Setup( c => c.Start() ).Callback( () => restarted.Add( 2 ) );

            var child3 = new Mock<IExecutor>();
            child3.Setup( c => c.Start() ).Callback( () => restarted.Add( 3 ) );

            var children = new List<IExecutor> { child1.Object, child2.Object, child3.Object };

            var oneForOne = new OneForOneNodeRestartStrategy();
            oneForOne.Restart( child2.Object, children );

            Assert.Equal( 1, restarted.Count );//"Only child2 should have been restarted"
            Assert.Equal( 2, restarted[0] );//"Only child2 should have been restarted"
        }

        [Fact]
        public void OneForAllStopsInOrderThenRestartsAllChildren()
        {
            var restarted = new List<int>();
            var stopped = new List<int>();

            var child1 = new Mock<IExecutor>();
            child1.Setup( c => c.Start() ).Callback( () => restarted.Add( 1 ) );
            child1.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => stopped.Add( 1 ) );

            var child2 = new Mock<IExecutor>();
            child2.Setup( c => c.Start() ).Callback( () => restarted.Add( 2 ) );
            child2.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => stopped.Add( 2 ) );

            var child3 = new Mock<IExecutor>();
            child3.Setup( c => c.Start() ).Callback( () => restarted.Add( 3 ) );
            child3.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => stopped.Add( 3 ) );

            var children = new List<IExecutor> { child1.Object, child2.Object, child3.Object };

            var oneForOne = new OneForAllNodeRestartStrategy();
            oneForOne.Restart( child2.Object, children );

            //Stop expected in reverse order
            Assert.Equal( 3, stopped.Count );//"All children should have been stopped"
            Assert.Equal( 3, stopped[0] );//"child3 should have been stopped 1st"
            Assert.Equal( 2, stopped[1] );//"child2 should have been stopped 2nd"
            Assert.Equal( 1, stopped[2] );//"child1 should have been stopped 3rd"

            //Restart in load order
            Assert.Equal( 3, restarted.Count );//"All children should have been restarted"
            Assert.Equal( 1, restarted[0] );//"child1 should have been started 1st"
            Assert.Equal( 2, restarted[1] );//"child2 should have been started 2nd"
            Assert.Equal( 3, restarted[2] );//"child3 should have been started 3rd"
        }

        [Fact]
        public void RestForOneStopsRestInOrderThenRestartsAllChildren_FromMiddle()
        {
            var restarted = new List<int>();
            var stopped = new List<int>();

            var child1 = new Mock<IExecutor>();
            child1.Setup( c => c.Start() ).Callback( () => restarted.Add( 1 ) );
            child1.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => stopped.Add( 1 ) );

            var child2 = new Mock<IExecutor>();
            child2.Setup( c => c.Start() ).Callback( () => restarted.Add( 2 ) );
            child2.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => stopped.Add( 2 ) );

            var child3 = new Mock<IExecutor>();
            child3.Setup( c => c.Start() ).Callback( () => restarted.Add( 3 ) );
            child3.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => stopped.Add( 3 ) );

            var children = new List<IExecutor> { child1.Object, child2.Object, child3.Object };

            var oneForOne = new RestForOneNodeRestartStrategy();
            oneForOne.Restart( child2.Object, children );

            //Stop expected in reverse order
            Assert.Equal( 2, stopped.Count );//"Only child2 and child3 should have been stopped"
            Assert.Equal( 3, stopped[0] );//"child3 should have been stopped 1st"
            Assert.Equal( 2, stopped[1] );//"child2 should have been stopped 2nd"

            //Restart in load order
            Assert.Equal( 2, restarted.Count );//"All child2 and child3 should have been restarted"
            Assert.Equal( 2, restarted[0] );//"child2 should have been started 1st"
            Assert.Equal( 3, restarted[1] );//"child3 should have been started 2nd"
        }

        [Fact]
        public void RestForOneStopsRestInOrderThenRestartsAllChildren_FromFirst()
        {
            var restarted = new List<int>();
            var stopped = new List<int>();

            var child1 = new Mock<IExecutor>();
            child1.Setup( c => c.Start() ).Callback( () => restarted.Add( 1 ) );
            child1.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => stopped.Add( 1 ) );

            var child2 = new Mock<IExecutor>();
            child2.Setup( c => c.Start() ).Callback( () => restarted.Add( 2 ) );
            child2.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => stopped.Add( 2 ) );

            var child3 = new Mock<IExecutor>();
            child3.Setup( c => c.Start() ).Callback( () => restarted.Add( 3 ) );
            child3.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => stopped.Add( 3 ) );

            var children = new List<IExecutor> { child1.Object, child2.Object, child3.Object };

            var oneForOne = new RestForOneNodeRestartStrategy();
            oneForOne.Restart( child1.Object, children );

            //Stop expected in reverse order
            Assert.Equal( 3, stopped.Count );//"All children should have been stopped"
            Assert.Equal( 3, stopped[0] );//"child3 should have been stopped 1st"
            Assert.Equal( 2, stopped[1] );//"child2 should have been stopped 2nd"
            Assert.Equal( 1, stopped[2] );//"child1 should have been stopped 3rd"

            //Restart in load order
            Assert.Equal( 3, restarted.Count );//"All children should have been restarted"
            Assert.Equal( 1, restarted[0] );//"child1 should have been started 1st"
            Assert.Equal( 2, restarted[1] );//"child2 should have been started 2nd"
            Assert.Equal( 3, restarted[2] );//"child3 should have been started 3rd"
        }

        [Fact]
        public void RestForOneStopsRestInOrderThenRestartsAllChildren_FromLast()
        {
            var restarted = new List<int>();
            var stopped = new List<int>();

            var child1 = new Mock<IExecutor>();
            child1.Setup( c => c.Start() ).Callback( () => restarted.Add( 1 ) );
            child1.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => stopped.Add( 1 ) );

            var child2 = new Mock<IExecutor>();
            child2.Setup( c => c.Start() ).Callback( () => restarted.Add( 2 ) );
            child2.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => stopped.Add( 2 ) );

            var child3 = new Mock<IExecutor>();
            child3.Setup( c => c.Start() ).Callback( () => restarted.Add( 3 ) );
            child3.Setup( c => c.ShutDown( It.IsAny<bool>() ) ).Callback( () => stopped.Add( 3 ) );

            var children = new List<IExecutor> { child1.Object, child2.Object, child3.Object };

            var oneForOne = new RestForOneNodeRestartStrategy();
            oneForOne.Restart( child3.Object, children );

            //Stop expected in reverse order
            Assert.Equal( 1, stopped.Count );//"Only child3 should have been stopped"
            Assert.Equal( 3, stopped[0] );//"child3 should have been stopped 1st"

            //Restart in load order
            Assert.Equal( 1, restarted.Count );//"Only child3 should have been restarted"
            Assert.Equal( 3, restarted[0] );//"child3 should have been started 1st"
        }
    }
}
