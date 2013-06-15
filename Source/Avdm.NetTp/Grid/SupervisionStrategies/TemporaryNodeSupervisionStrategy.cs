using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;

namespace Avdm.NetTp.Grid.SupervisionStrategies
{
    /// <summary>
    /// a temporary process is a process that should never be restarted. 
    /// They are for short-lived workers that are expected to fail and which have 
    /// few bits of code who depend on them.
    /// </summary>
    public class TemporaryNodeSupervisionStrategy : NodeSupervisionStrategy
    {
        public override NodeExitAction ChildExited( IExecutor child, bool succeeded )
        {
            return NodeExitAction.Ignore;
        }
    }
}