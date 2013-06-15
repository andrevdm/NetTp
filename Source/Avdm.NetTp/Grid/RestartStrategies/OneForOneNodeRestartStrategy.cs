using System.Collections.Generic;
using Avdm.NetTp.Grid.Executors;

namespace Avdm.NetTp.Grid.RestartStrategies
{
    /// <summary>
    /// If one child process terminates and should be restarted, 
    /// only that child process is affected.
    /// </summary>
    public class OneForOneNodeRestartStrategy : NodeRestartStrategy
    {
        public override void Restart( IExecutor child, List<IExecutor> children )
        {
            child.Start();
        }
    }
}