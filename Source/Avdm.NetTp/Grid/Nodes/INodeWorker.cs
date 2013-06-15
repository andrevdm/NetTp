using System.Threading;

namespace Avdm.NetTp.Grid.Nodes
{
    public interface INodeWorker
    {
        void RunWorker( Node node, CancellationToken cancellation );
    }
}
