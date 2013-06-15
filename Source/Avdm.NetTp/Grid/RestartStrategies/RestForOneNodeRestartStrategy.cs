using System.Collections.Generic;
using System.Linq;
using Avdm.NetTp.Grid.Executors;

namespace Avdm.NetTp.Grid.RestartStrategies
{
    /// <summary>
    /// If one child process terminates and should be restarted, the 'rest' of the child processes 
    /// I.e. the child processes after the terminated child process in the start order -- are terminated. 
    /// Then the terminated child process and all child processes after it are restarted.
    /// </summary>
    public class RestForOneNodeRestartStrategy : NodeRestartStrategy
    {
        public override void Restart( IExecutor child, List<IExecutor> childrenList )
        {
            List<IExecutor> children;

            lock( childrenList )
            {
                children = childrenList.ToList();
            }

            var at = children.FindIndex( c => c == child );

            if( at == -1 )
            {
                return;
            }

            foreach( var c in children.Skip( at ).Reverse() )
            {
                c.ShutDown( false );
            }

            foreach( var c in children.Skip( at ) )
            {
                c.Start();
            }
        }
    }
}