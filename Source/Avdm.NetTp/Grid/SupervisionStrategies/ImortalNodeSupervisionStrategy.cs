using System;
using System.Collections.Concurrent;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;

namespace Avdm.NetTp.Grid.SupervisionStrategies
{
    /// <summary>
    /// An importal node should always be restarted, no matter what. 
    /// No max restart is ever applied
    /// </summary>
    [Serializable]
    public class ImortalNodeSupervisionStrategy : NodeSupervisionStrategy
    {
        public TimeSpan ResetTime { get; set; }
        public int[] DelayMsTimes { get; set; }
        private readonly ConcurrentDictionary<Guid, ChildRestartDelayer> m_history = new ConcurrentDictionary<Guid, ChildRestartDelayer>();

        protected ImortalNodeSupervisionStrategy()
        {
        }

        public ImortalNodeSupervisionStrategy( TimeSpan? resetTime = null, int[] delayMsTimes = null )
        {
            ResetTime = resetTime ?? TimeSpan.FromSeconds( 61 );
            DelayMsTimes = delayMsTimes;
        }

        public override NodeExitAction ChildExited( IExecutor child, bool succeeded )
        {
            ChildRestartDelayer delayer;

            if( !m_history.TryGetValue( child.Id, out delayer ) )
            {
                delayer = new ChildRestartDelayer( child.Id, uint.MaxValue, ResetTime, DelayMsTimes );
                m_history[child.Id] = delayer;
            }

            delayer.Next();
            return NodeExitAction.Restart;
        }
    }
}