using System;

namespace Avdm.NetTp.Grid.Nodes
{
    public interface INodeRemoteControl
    {
        void ShutDown( Guid nodeId, bool success = false );
        void ShutDownAll( bool success = false );
    }
}
