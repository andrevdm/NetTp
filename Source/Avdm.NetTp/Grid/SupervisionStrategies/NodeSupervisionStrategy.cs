using System;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;

namespace Avdm.NetTp.Grid.SupervisionStrategies
{
    public abstract class NodeSupervisionStrategy
    {
        /// <summary>
        /// A permanent process should always be restarted, no matter what. 
        /// This is usually used by vital, long-living processes (or services) running on your node.
        /// </summary>
        public static PermanentNodeSupervisionStrategy Permanent( uint maxRestarts, TimeSpan maxTime, int[] delayMsTimes = null )
        {
            return new PermanentNodeSupervisionStrategy( maxRestarts, maxTime, delayMsTimes );
        }

        /// <summary>
        /// A permanent process should always be restarted, no matter what. 
        /// This is usually used by vital, long-living processes (or services) running on your node.
        /// </summary>
        public static PermanentNodeSupervisionStrategy DefaultPermanent { get; private set; }

        /// <summary>
        /// a temporary process is a process that should never be restarted. 
        /// They are for short-lived workers that are expected to fail and which have 
        /// few bits of code who depend on them.
        /// </summary>
        public static TemporaryNodeSupervisionStrategy DefaultTemporary { get; private set; }


        /// <summary>
        /// An importal node should always be restarted, no matter what. 
        /// No max restart is ever applied
        /// </summary>
        public static ImortalNodeSupervisionStrategy DefaultImortal { get; private set; }

        /// <summary>
        /// An importal node should always be restarted, no matter what. 
        /// No max restart is ever applied
        /// </summary>
        public static ImortalNodeSupervisionStrategy Imortal( TimeSpan resetTime, int[] delayMsTimes = null )
        {
            return new ImortalNodeSupervisionStrategy( resetTime, delayMsTimes );
        }

        /// <summary>
        /// Transient nodes are meant to run until they terminate normally and then they won't be restarted. 
        /// However, if they die of abnormal causes (exit reason is anything but normal), they're 
        /// going to be restarted. This restart option is often used for workers that need to succeed 
        /// at their task, but won't be used after they do so.
        /// </summary>
        public static TransientNodeSupervisionStrategy DefaultTransient { get; private set; }

        /// <summary>
        /// Transient nodes are meant to run until they terminate normally and then they won't be restarted. 
        /// However, if they die of abnormal causes (exit reason is anything but normal), they're 
        /// going to be restarted. This restart option is often used for workers that need to succeed 
        /// at their task, but won't be used after they do so.
        /// </summary>
        public static TransientNodeSupervisionStrategy Transient( uint maxRestarts, TimeSpan resetTime, int[] delayMsTimes = null )
        {
            return new TransientNodeSupervisionStrategy( maxRestarts, resetTime, delayMsTimes );
        }

        static NodeSupervisionStrategy()
        {
            DefaultTemporary = new TemporaryNodeSupervisionStrategy();
            DefaultImortal = new ImortalNodeSupervisionStrategy();
            DefaultTransient = new TransientNodeSupervisionStrategy();
            DefaultPermanent = new PermanentNodeSupervisionStrategy( 12, TimeSpan.FromSeconds( 61 ), null );
        }

        public abstract NodeExitAction ChildExited( IExecutor child, bool succeeded );
    }
}