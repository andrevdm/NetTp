using System.Collections.Generic;
using Avdm.Core.Patterns;
using Avdm.NetTp.Core;
using Avdm.NetTp.Grid.NodeResponsibilityHandlers;

namespace Avdm.NetTp.Grid.Nodes
{
    public class DefaultNodeResponsabilityProvider : INodeResponsabilityProvider
    {
        public IEnumerable<IChainOfResponsibilityHandler<Node, object>> Load( Node client )
        {
            return new IChainOfResponsibilityHandler<Node, object>[]
                {
                    new NodeLoggingHandler( client ), 
                    new NodeRemoteControlHandler( client ), 
                    new NodeMonitorHandler( client ),
                    new NodeHeartbeatHandler( client ),
                };
        }
    }
}