using System;
using System.Collections.Concurrent;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;

namespace Avdm.NetTp.Grid.SupervisionStrategies
{
    /// <summary>
    /// Transient nodes are meant to run until they terminate normally and then they won't be restarted. 
    /// However, if they die of abnormal causes (exit reason is anything but normal), they're 
    /// going to be restarted. This restart option is often used for workers that need to succeed 
    /// at their task, but won't be used after they do so.
    /// </summary>
    [Serializable]
    public class TransientNodeSupervisionStrategy : NodeSupervisionStrategy
    {
        public TimeSpan ResetTime { get; set; }
        public uint MaxRestarts { get; set; }
        public int[] DelayMsTimes { get; set; }
        private readonly ConcurrentDictionary<Guid, ChildRestartDelayer> m_history = new ConcurrentDictionary<Guid, ChildRestartDelayer>();

        protected TransientNodeSupervisionStrategy()
        {
        }

        public TransientNodeSupervisionStrategy( uint maxRestarts = uint.MaxValue, TimeSpan? resetTime = null, int[] delayMsTimes = null )
        {
            ResetTime = resetTime ?? TimeSpan.FromSeconds( 61 );
            MaxRestarts = maxRestarts;
            DelayMsTimes = delayMsTimes;
        }

        public override NodeExitAction ChildExited( IExecutor child, bool succeeded )
        {
            if( succeeded )
            {
                return NodeExitAction.Ignore;
            }

            ChildRestartDelayer delayer;

            if( !m_history.TryGetValue( child.Id, out delayer ) )
            {
                delayer = new ChildRestartDelayer( child.Id, MaxRestarts, ResetTime, DelayMsTimes );
                m_history[child.Id] = delayer;
            }

            return delayer.Next();
        }
    }
}