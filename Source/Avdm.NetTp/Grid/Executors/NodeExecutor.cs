using System;
using System.Threading;
using Avdm.Core;
using Avdm.NetTp.Core;
using Avdm.NetTp.Grid.Nodes;

namespace Avdm.NetTp.Grid.Executors
{
    public class NodeExecutor : IExecutor
    {
        private readonly Func<Node> m_nodeFactory;
        private readonly Action<Node,CancellationToken> m_nodeAction;
        private Node m_node;
        public string Type { get { return "Node"; } }
        public object ChildId { get { return m_node.Id; } }
        public string ChildName { get { return m_node.NodeName; } }

        public event ExecutorExitedHandler Exited;
        public Guid Id { get; private set; }

        public NodeExecutor( Func<Node> nodeFactory, Action<Node,CancellationToken> nodeAction )
        {
            Preconditions.CheckNotNull( nodeFactory, "nodeFactory" );
            Preconditions.CheckNotNull( nodeAction, "nodeAction" );

            Id = Guid.NewGuid();

            m_nodeFactory = nodeFactory;
            m_nodeAction = nodeAction;
        }

        public bool IsExecuting { get { return m_node != null && m_node.WorkerExecuting; } }

        public void Start()
        {
            if( m_node == null )
            {
                m_node = m_nodeFactory();
                m_node.NodeEnded += NodeExited;
                m_node.StartWorker( m_nodeAction );
            }
        }

        public void ShutDown( bool succeeded )
        {
            if( m_node != null )
            {
                m_node.NodeEnded -= NodeExited;
                m_node.ShutDown( succeeded );
                m_node = null;
            }
        }
        
        private void NodeExited( object sender, ExecutorExitedEventArgs e )
        {
            if( m_node != null )
            {
                m_node.NodeEnded -= NodeExited;
                m_node = null;
            }

            Exited( this, e );
        }

        ~NodeExecutor()
        {
            Dispose( false );
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( Boolean disposing )
        {
            if( disposing )
            {
                ShutDown( true );

                if( m_node != null )
                {
                    m_node.Dispose();
                    m_node = null;
                }
            }
        }
    }
}