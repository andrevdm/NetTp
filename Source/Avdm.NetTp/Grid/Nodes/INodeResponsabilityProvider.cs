using System.Collections.Generic;
using Avdm.Core.Patterns;
using Avdm.NetTp.Core;

namespace Avdm.NetTp.Grid.Nodes
{
    public interface INodeResponsabilityProvider
    {
        IEnumerable<IChainOfResponsibilityHandler<Node, object>> Load( Node client );
    }
}