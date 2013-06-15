using System;
using System.Diagnostics;
using System.Threading;
using Avdm.Config;
using Avdm.Core;
using Avdm.Core.Di;
using Avdm.Core.Patterns;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Messaging;
using StructureMap;

namespace Avdm.NetTp.Grid.NodeResponsibilityHandlers
{
    public class NodeMonitorHandler : IChainOfResponsibilityHandler<Node,object>
    {
        private readonly Node m_client;
        private readonly Timer m_timer;
        private readonly INetTpMessageBus m_bus;
        private readonly TimeSpan m_period;
        private readonly IClock m_clock;

        public NodeMonitorHandler( Node client )
        {
            Preconditions.CheckNotNull( client, "client" );

            m_period = TimeSpan.FromSeconds( int.Parse( ConfigManager.AppSettings["NodeMonitor.PeriodSeconds"] ?? "5" ) );

            m_clock = ObjectFactory.GetInstance<IClock>();
            m_bus = ObjectFactory.GetInstance<INetTpMessageBus>();
            m_client = client;
            m_timer = new Timer( 
                Tick, 
                null, 
                TimeSpan.FromMilliseconds( 5 ), 
                m_period );

            SendMonitorEvent();
        }

        public bool Handle( Node client, NodeActions.ShutDown data, bool wasHandled )
        {
            m_timer.Dispose();
            return true;
        }

        public bool Handle( Node client, NodeActions.Started data, bool wasHandled )
        {
            SendMonitorEvent();
            return false;
        }

        public bool Handle( Node client, object data, bool wasHandled )
        {
            return false;
        }

        private void Tick( object state )
        {
            SendMonitorEvent();
        }

        private void SendMonitorEvent()
        {
            var currentProcess = Process.GetCurrentProcess();

            var msg = new NodeMonitorUpdateEventMessage(
                Environment.MachineName,
                currentProcess.Id,
                currentProcess.ProcessName,
                m_client.Id,
                m_client.NodeName,
                m_client.WorkerExecuting,
                m_client.NodeWorkerStrategy,
                m_client.SupervisionStrategy,
                m_client.RestartStrategy,
                m_client.GetExecutorsInfo() );

            msg.ExpireAt = m_clock.Now.Add( m_period );

            m_bus.PublishEvent( msg );
        }
    }
}
