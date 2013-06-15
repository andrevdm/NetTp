using System;
using System.Threading;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.Pool;

namespace Avdm.NetTp.Grid.Config
{
    public class ConfigHotSwapNodeWorker : INodeWorker
    {
        public void RunWorker( Node node, CancellationToken cancellation )
        {
            Console.WriteLine( 
                "ConfigHotSwapNodeWorker: HotSwapLoaderType={0}, HotSwapLoaderTypeAsmToScan={1}", 
                node.NodeSettings["HotSwapLoaderType"], 
                node.NodeSettings["HotSwapLoaderTypeAsmToScan"] );

            var pool = new HotSwappableHandlerPool( 
                node, 
                node.NodeSettings["HotSwapLoaderType"], 
                node.NodeSettings["HotSwapLoaderTypeAsmToScan"] );

            cancellation.WaitHandle.WaitOne();
        }
    }
}
