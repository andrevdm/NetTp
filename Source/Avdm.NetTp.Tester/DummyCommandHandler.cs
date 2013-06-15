using System;
using Avdm.NetTp.Grid.Pool;

namespace Avdm.NetTp.Tester
{
    public class DummyCommandHandler : IHandleCommand<DummyCommand>, IHandleCommand<DummyCommand2>
    {
        private static int g_count = 0;

        public void HandleCommand( DummyCommand command )
        {
            Console.WriteLine( "c2: {0}, {1}", command.Message, ++g_count );
        }

        public void HandleCommand( DummyCommand2 command )
        {
            Console.WriteLine( "c2:[2] {0}, {1}", command.Message, ++g_count );
        }
    }
}