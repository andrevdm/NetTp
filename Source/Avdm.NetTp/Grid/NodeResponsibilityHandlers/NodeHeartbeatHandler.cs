using System;
using System.Diagnostics;
using System.Threading;
using Avdm.Config;
using Avdm.Core;
using Avdm.Core.Patterns;
using Avdm.NetTp.Grid.Nodes;
using Sider;
using StructureMap;

namespace Avdm.NetTp.Grid.NodeResponsibilityHandlers
{
    public class NodeHeartbeatHandler : IChainOfResponsibilityHandler<Node,object>
    {
        private readonly Node m_client;
        private readonly Timer m_timer;
        private readonly TimeSpan m_period;
        private readonly Process m_currentProcess;

        public NodeHeartbeatHandler( Node client )
        {
            Preconditions.CheckNotNull( client, "client" );

            m_currentProcess = Process.GetCurrentProcess();

            m_period = TimeSpan.FromSeconds( int.Parse( ConfigManager.AppSettings["NodeHeartbeatHandler.PeriodSeconds"] ?? "1" ) );

            m_client = client;
            m_timer = new Timer( Tick, null, TimeSpan.FromMilliseconds( 5 ), m_period );

            SendHeartbeat();
        }

        public bool Handle( Node client, NodeActions.ShutDown data, bool wasHandled )
        {
            m_timer.Dispose();
            return true;
        }

        public bool Handle( Node client, NodeActions.Initalising data, bool wasHandled )
        {
            SendHeartbeat();
            return false;
        }
        
        public bool Handle( Node client, NodeActions.Started data, bool wasHandled )
        {
            SendHeartbeat();
            return false;
        }

        public bool Handle( Node client, object data, bool wasHandled )
        {
            return false;
        }

        private void Tick( object state )
        {
            SendHeartbeat();
        }

        private void SendHeartbeat()
        {
            var redis = ObjectFactory.GetInstance<IRedisClient<string>>();

            string key = Node.MakeNodeHeartbeatName( m_client.ApplicationName, m_client.NodeName );

            redis.Pipeline( r =>
                {
                    r.HSet( key, "pid", m_currentProcess.Id.ToString() );
                    r.HSet( key, "gid", m_client.Id.ToString() );
                    r.HSet( key, "workerExecuting", m_client.WorkerExecuting.ToString() );
                    r.HSet( key, "processName", m_currentProcess.ProcessName );

                    r.Expire( key, TimeSpan.FromSeconds( (int)(m_period.TotalSeconds + 1 ) ) );
                } );
        }
    }
}
