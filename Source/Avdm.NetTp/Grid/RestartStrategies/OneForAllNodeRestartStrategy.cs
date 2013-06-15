using System.Collections.Generic;
using System.Linq;
using Avdm.NetTp.Grid.Executors;

namespace Avdm.NetTp.Grid.RestartStrategies
{
    /// <summary>
    /// If one child process terminates and should be restarted, 
    /// all other child processes are terminated and then all child processes are restarted
    /// </summary>
    public class OneForAllNodeRestartStrategy : NodeRestartStrategy
    {
        public override void Restart( IExecutor child, List<IExecutor> childrenList )
        {
            IExecutor[] children;

            lock( childrenList )
            {
                children = childrenList.ToArray();
            }

            //Stop expected in reverse order
            foreach( var c in children.Reverse() )
            {
                //TODO must not call Exited
                c.ShutDown( false );
            }

            //Restart in load order
            foreach( var c in children )
            {
                c.Start();
            }
        }
    }
}