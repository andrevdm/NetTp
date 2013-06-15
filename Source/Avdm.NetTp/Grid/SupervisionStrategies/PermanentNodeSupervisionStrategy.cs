using System;
using System.Collections.Concurrent;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;

namespace Avdm.NetTp.Grid.SupervisionStrategies
{
    /// <summary>
    /// A permanent process should always be restarted, no matter what. 
    /// This is usually used by vital, long-living processes (or services) running on your node.
    /// However if it is restarted more than maxRestart times in maxTime then the child is considered
    /// to have failed
    /// </summary>
    [Serializable]
    public class PermanentNodeSupervisionStrategy : NodeSupervisionStrategy
    {
        public uint MaxRestarts { get; set; }
        public TimeSpan MaxTime { get; set; }
        public int[] DelayMsTimes { get; set; }
        public readonly ConcurrentDictionary<Guid, ChildRestartDelayer> m_history = new ConcurrentDictionary<Guid, ChildRestartDelayer>();

        protected PermanentNodeSupervisionStrategy()
        {
        }

        public PermanentNodeSupervisionStrategy( uint maxRestarts, TimeSpan maxTime, int[] delayMsTimes = null )
        {
            MaxRestarts = maxRestarts > 0 ? maxRestarts : uint.MaxValue;
            MaxTime = maxTime;
            DelayMsTimes = delayMsTimes;
        }

        public override NodeExitAction ChildExited( IExecutor child, bool succeeded )
        {
            ChildRestartDelayer delayer;
            
            if( !m_history.TryGetValue( child.Id, out delayer ) )
            {
                delayer = new ChildRestartDelayer( child.Id, MaxRestarts, MaxTime, DelayMsTimes );
                m_history[child.Id] = delayer;
            }

            return delayer.Next();
        }
    }
}