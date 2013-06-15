using System.Collections.Generic;
using Avdm.NetTp.Grid.Executors;

namespace Avdm.NetTp.Grid.RestartStrategies
{
    /// <summary>
    /// Defines the supervisor's restart strategies
    /// See the NodeSupervisionStrategy which defines if a node should be restarted.
    /// </summary>
    public abstract class NodeRestartStrategy
    {
        static NodeRestartStrategy()
        {
            OneForOne = new OneForOneNodeRestartStrategy();
            OneForAll = new OneForAllNodeRestartStrategy();
            RestForOne = new RestForOneNodeRestartStrategy();
        }

        /// <summary>
        /// If one child process terminates and should be restarted, 
        /// only that child process is affected.
        /// </summary>
        public static OneForOneNodeRestartStrategy OneForOne { get; private set; }

        /// <summary>
        /// If one child process terminates and should be restarted, 
        /// all other child processes are terminated and then all child processes are restarted
        /// </summary>
        public static OneForAllNodeRestartStrategy OneForAll { get; private set; }

        /// <summary>
        /// If one child process terminates and should be restarted, the 'rest' of the child processes 
        /// I.e. the child processes after the terminated child process in the start order -- are terminated. 
        /// Then the terminated child process and all child processes after it are restarted.
        /// </summary>
        public static RestForOneNodeRestartStrategy RestForOne { get; private set; }

        public abstract void Restart( IExecutor child,List<IExecutor> children );
    }
}