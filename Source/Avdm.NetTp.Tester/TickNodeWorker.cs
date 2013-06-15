using System;
using System.Threading;
using Avdm.NetTp.Grid.Nodes;

namespace Avdm.NetTp.Tester
{
    public class TickNodeWorker : INodeWorker
    {
        public void RunWorker( Node node, CancellationToken cancellation )
        {
            using( var timer = new Timer( Tick, null, 200, 1000 ) )
            {
                cancellation.WaitHandle.WaitOne();
            }
        }

        private void Tick( object state )
        {
            Console.WriteLine( "Tick: {0}", DateTime.Now );
        }
    }
}
