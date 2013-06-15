/*using Xunit;
using NetTp.Messaging;

namespace NetTp.UnitTests.Messaging
{
    
    public class InProcMessageBusImplTests
    {
        [Fact]
        public void SubscribeAndDistributeEvents()
        {
            var gotEvent = 0;

            var bus = new InProcMessageBusImpl();
            bus.SubscribeToEvent<DummyEvent>( "test1", e => gotEvent++ );
            bus.SubscribeToEvent<DummyEvent>( "test2", e => gotEvent *= 10 );
            bus.PublishEvent<DummyEvent>( publish => publish( new DummyEvent() ) );

            Assert.Equal( 10, gotEvent, "Event should have been recieved by both handlers" );
        }

        [Fact]
        public void SubscribeAndDistributeCommand()
        {
            var gotCommand = 0;

            var bus = new InProcMessageBusImpl();
            bus.SubscribeToCommand<DummyCommand>( "test", e => gotCommand++ );
            bus.PublishCommand<DummyCommand>( publish => publish( new DummyCommand() ) );

            Assert.Equal( 1, gotCommand, "Command should have been recieved" );
        }

        [Fact]
        public void SubscribeAndDistributeQuery()
        {
            var gotQuery = 0;

            var bus = new InProcMessageBusImpl();
            bus.SubscribeToRpc<DummyQueryRequest, DummyQueryResponse>( "test", e =>
            {
                gotQuery++;
                return new DummyQueryResponse();
            } );

            DummyQueryResponse response = null;
            bus.Rpc<DummyQueryRequest, DummyQueryResponse>( rpc => response = rpc( new DummyQueryRequest() ) );

            Assert.Equal( 1, gotQuery, "Query should have been recieved" );
            Assert.IsNotNull( response, "Expecting a valid response" );
        }
    }
}
*/