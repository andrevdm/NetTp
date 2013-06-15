using System;
using System.Diagnostics;

namespace Avdm.NetTp.Grid.Nodes
{
    public interface INodeFinder
    {
        Process FindNodeProcessByNodeName( string applicationName, string nodeName );
        Guid FindLocalNodeIdByName( string applicationName, string nodeName );
    }
}
