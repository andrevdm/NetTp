using System;
using Avdm.Core.Di;
using Avdm.NetTp.Core;
using Avdm.NetTp.Grid.Nodes;
using StructureMap;

namespace Avdm.NetTp.Grid.SupervisionStrategies
{
    public class ChildRestartDelayer
    {
        private readonly uint m_maxRestarts;
        private readonly TimeSpan m_maxTime;
        private readonly int[] m_delayMsTimes;
        private DateTime m_lastRestart;
        private int m_waitPosition;
        private int m_restartsInTimespan;

        public Guid Id { get; private set; }
        public int TotalRestartCount { get; private set; }

        public ChildRestartDelayer( Guid id, uint maxRestarts, TimeSpan maxTime, int[] delayMsTimes = null )
        {
            Id = id;
            m_lastRestart = DateTime.MinValue;
            m_waitPosition = -1;

            m_maxRestarts = maxRestarts > 0 ? maxRestarts : uint.MaxValue;
            m_maxTime = maxTime;
            m_delayMsTimes = delayMsTimes ?? new[] { 0, 10, 100, 200, 500, 1000, 1500, 4000, 8000, 16000, 32000, 60000 };
        }

        public NodeExitAction Next()
        {
            var clock = ObjectFactory.GetInstance<IClock>();

            if( (clock.Now - m_lastRestart) > m_maxTime )
            {
                m_restartsInTimespan = 0;
                m_waitPosition = -1;
            }

            m_lastRestart = DateTime.Now;
            TotalRestartCount++;
            m_waitPosition = Math.Min( m_delayMsTimes.Length - 1, m_waitPosition + 1 );
            m_restartsInTimespan++;

            if( m_restartsInTimespan > m_maxRestarts )
            {
                return NodeExitAction.Fail;
            }

            if( m_delayMsTimes[m_waitPosition] > 0 )
            {
                clock.ThreadSleep( m_delayMsTimes[m_waitPosition] );
            }

            return NodeExitAction.Restart;
        }
    }
}