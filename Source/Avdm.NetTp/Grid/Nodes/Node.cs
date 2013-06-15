using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avdm.Core;
using Avdm.Core.Console;
using Avdm.Core.Di;
using Avdm.Core.Patterns;
using Avdm.Core.Logging;
using Avdm.NetTp.Grid.Config;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;
using Avdm.NetTp.Grid.Triad;
using StructureMap;

namespace Avdm.NetTp.Grid.Nodes
{
    /// <summary>
    /// A node contains the executing worker. It also acts as the supervisor for children
    /// 
    /// A supervisor is responsible for starting, stopping and monitoring its child processes. 
    /// The basic idea of a supervisor is that it should keep its child processes alive by restarting them 
    /// when necessary.
    /// 
    /// Supervisors are used to build an hierarchical process structure called a supervision tree, 
    /// a nice way to structure a fault tolerant application. 
    /// </summary>
    public class Node : IDisposable
    {
        private bool m_nodeActionStarted;
        private CancellationTokenSource m_cancellationSource = new CancellationTokenSource();
        private readonly object m_sync = new object();
        private readonly List<IExecutor> m_children = new List<IExecutor>();
        private readonly ChainOfResponsibility<Node, object> m_chainOfResponsibility;
        private bool m_hasExited = false;

        public Guid Id { get; private set; }
        public bool WorkerExecuting { get; private set; }
        public CancellationToken CancellationToken { get; private set; }
        public string ApplicationName { get; private set; }
        public string NodeName { get; private set; }
        public NodeWorkerStrategy NodeWorkerStrategy { get; private set; }
        public NodeSupervisionStrategy SupervisionStrategy { get; private set; }
        public NodeRestartStrategy RestartStrategy { get; private set; }
        public Dictionary<string, string> NodeSettings { get; set; }

        public event EventHandler WorkerStarted = delegate { };
        public event ExecutorExitedHandler WorkerEnded;
        public event ExecutorExitedHandler NodeEnded = delegate { };

        public Node(
            string applicationName,
            string nodeName,
            NodeWorkerStrategy nodeWorkerStrategy,
            NodeRestartStrategy restartStrategy,
            NodeSupervisionStrategy supervisionStrategy )
        {
            Preconditions.CheckNotBlank( nodeName, "nodeName" );
            Preconditions.CheckNotBlank( nodeName, "applicationName" );

            Id = Guid.NewGuid();
            m_chainOfResponsibility = new ChainOfResponsibility<Node, object>( this );
            ApplicationName = applicationName;
            NodeName = nodeName;
            NodeSettings = new Dictionary<string, string>();
            NodeWorkerStrategy = nodeWorkerStrategy;
            SupervisionStrategy = supervisionStrategy;
            RestartStrategy = restartStrategy;
            CancellationToken = m_cancellationSource.Token;
            WorkerEnded += OnWorkerEnded;

            var handlerProvider = ObjectFactory.GetInstance<INodeResponsabilityProvider>();
            var handlers = handlerProvider.Load( this );
            if( handlers != null )
            {
                foreach( var handler in handlers )
                {
                    m_chainOfResponsibility.AddHandler( handler );
                }
            }

            PrintNodeInfo();
        }

        public Node( string applicationName, NodeConfig config, Action<Node> preLoad = null )
            : this( applicationName, config.Name, config.WorkerStrategy, config.RestartStrategy, config.SupervisionStrategy )
        {
            NodeSettings = config.NodeSettings;

            m_chainOfResponsibility.Handle( new NodeActions.Initalising() );

            if( preLoad != null )
            {
                preLoad( this );
            }

            SetupChildrenFromConfig( config );
            SetupWorkerFromConfig( config );
        }

        public void StartWorker( Action<Node, CancellationToken> nodeAction )
        {
            lock( m_sync )
            {
                if( m_nodeActionStarted )
                {
                    throw new InvalidOperationException( "Node action already started" );
                }

                m_nodeActionStarted = true;
            }

            if( m_children.Count > 0 )
            {
                Log.WarnFormat( "A node should not have children and a worker. App={0}, name={1}", ApplicationName, NodeName );
            }

            RunNodeAction( nodeAction );
            m_chainOfResponsibility.Handle( new NodeActions.Started() );
        }

        public void ShutDown( bool succeeded )
        {
            try
            {
                m_hasExited = true;

                IExecutor[] children;
                lock( m_children )
                {
                    children = m_children.ToArray();
                }

                foreach( var c in children.Reverse() )
                {
                    var child = c;
                    child.Exited -= ChildExecutorExited;

                    lock( m_children )
                    {
                        m_children.Remove( child );
                    }

                    child.ShutDown( succeeded );
                }
            }
            finally
            {
                m_cancellationSource.Cancel();

                NodeEnded( this, new ExecutorExitedEventArgs( succeeded ) );
            }

            m_chainOfResponsibility.Handle( new NodeActions.ShutDown() );
        }

        public void Supervise( NodeConfig config )
        {
            Supervise( () => new Node( ApplicationName, config ), null );
        }

        public void Supervise( Func<Node> nodeFactory, Action<Node, CancellationToken> nodeAction )
        {
            Supervise( new NodeExecutor( nodeFactory, nodeAction ) );
        }

        public void Supervise( IExecutor child )
        {
            Preconditions.CheckNotNull( child, "child" );

            lock( m_children )
            {
                m_children.Add( child );
            }

            child.Exited += ChildExecutorExited;

            if( !child.IsExecuting )
            {
                child.Start();
            }

            m_chainOfResponsibility.Handle( new NodeActions.Supervising( child ) );

            if( m_nodeActionStarted )
            {
                Log.WarnFormat( "A node should not have children and a worker. App={0}, name={1}", ApplicationName, NodeName );
            }
        }

        public IEnumerable<IExecutorInfo> GetExecutorsInfo()
        {
            return m_children.ConvertAll( e => new ExecutorInfo( e.Id, e.Type, e.ChildId, e.ChildName ) );
        }

        private void SetupWorkerFromConfig( NodeConfig config )
        {
            if( config == null )
            {
                return;
            }

            if( string.IsNullOrWhiteSpace( config.Worker ) )
            {
                return;
            }

            var workerType = Type.GetType( config.Worker, true );
            var worker = (INodeWorker)Activator.CreateInstance( workerType );
            StartWorker( worker.RunWorker );
        }

        private void SetupChildrenFromConfig( NodeConfig config )
        {
           SetupChildNodes( config );
           SetupChildProcesses( config );
        }

        private void SetupChildNodes( NodeConfig config )
        {
            if( config.Nodes == null )
            {
                return;
            }

            foreach( var childL in config.Nodes )
            {
                var child = childL;

                if( child.IsProcess )
                {
                    Supervise( new ProcessExecutor(
                                   child.Name,
                                   null,
                                   null,
                                   CreateNodeProcessStartInfo( typeof(ConfigNodeFactory).FullName, ApplicationName, "*" + child.ConfigId.ToString() ),
                                   () =>
                                       {
                                           var nodeFinder = ObjectFactory.GetInstance<INodeFinder>();
                                           return nodeFinder.FindNodeProcessByNodeName( ApplicationName, child.Name );
                                       } ) );
                }
                else
                {
                    Supervise( config );
                }
            }
        }

        private void SetupChildProcesses( NodeConfig config )
        {
            if( config.Processes == null )
            {
                return;
            }

            foreach( var child in config.Processes )
            {
                if( string.IsNullOrWhiteSpace( child.Name ) )
                {
                    child.Name = Path.GetFileNameWithoutExtension( child.FileName );
                }

                var psi = new ProcessStartInfo();
                psi.FileName = child.FileName;
                psi.Arguments = psi.Arguments;
                psi.WorkingDirectory = child.WorkingDirectory;
                Supervise( new ProcessExecutor( string.Format( "{0}:{1}:{2}", ApplicationName, NodeName, child.Name ), null, null, psi, null ) );
            }
        }

        private void ChildExecutorExited( object childObj, ExecutorExitedEventArgs e )
        {
            var child = (IExecutor)childObj;

            lock( m_children )
            {
                if( !m_children.Contains( child ) )
                {
                    return;
                }
            }

            //Dont supervise if exiting
            if( m_hasExited )
            {
                return;
            }

            var exitAction = SupervisionStrategy.ChildExited( child, e.Succeeded );

            switch( exitAction )
            {
                case NodeExitAction.Restart:
                    HandleChildRestart( child );
                    break;

                case NodeExitAction.Fail:
                    HandleChildFail( child );
                    break;

                case NodeExitAction.Ignore:
                    HandleChildIgnore( child );
                    break;

                default:
                    throw new InvalidOperationException( "Unknown exit action " + exitAction );
            }
        }

        private void HandleChildIgnore( IExecutor child )
        {
            child.Exited -= ChildExecutorExited;

            lock( m_children )
            {
                m_children.Remove( child );
            }

            m_chainOfResponsibility.Handle( new NodeActions.ChildClosed( child ) );
        }

        private void HandleChildFail( IExecutor child )
        {
            m_chainOfResponsibility.Handle( new NodeActions.ChildFailed( child ) );
            ShutDown( false );
        }

        private void HandleChildRestart( IExecutor child )
        {
            RestartStrategy.Restart( child, m_children );
            m_chainOfResponsibility.Handle( new NodeActions.ChildRestarted( child ) );
        }

        private void RunNodeAction( Action<Node, CancellationToken> nodeAction )
        {
            Task.Factory.StartNew( () =>
                {
                    WorkerExecuting = true;
                    WorkerStarted( this, EventArgs.Empty );

                    try
                    {
                        nodeAction( this, CancellationToken );
                        WorkerExecuting = false;
                        WorkerEnded( this, new ExecutorExitedEventArgs( true ) );
                    }
                    catch( Exception ex )
                    {
                        Log.Error( ex );

                        WorkerExecuting = false;
                        WorkerEnded( this, new ExecutorExitedEventArgs( false, ex ) );
                    }
                } );
        }

        private void OnWorkerEnded( object sender, ExecutorExitedEventArgs e )
        {
            m_chainOfResponsibility.Handle( new NodeActions.WorkerEnded() );

            if( NodeWorkerStrategy == NodeWorkerStrategy.Supervise )
            {
                ShutDown( false );
            }
        }

        private void PrintNodeInfo()
        {
            ConsoleAsync.WriteLine( ConsoleColor.DarkGray, @"

Node starting
   +----------------------+--------------------------------------
   | Application name     | {0}
   +----------------------+--------------------------------------
   | Name                 | {1}
   +----------------------+--------------------------------------
   | Supervision strategy | {2}
   +----------------------+--------------------------------------
   | Restart strategy     | {3}
   +----------------------+--------------------------------------
   | Worker strategy      | {4}
   +----------------------+--------------------------------------


", ApplicationName, NodeName, SupervisionStrategy.GetType().Name, RestartStrategy.GetType().Name, NodeWorkerStrategy );
        }


        ~Node()
        {
            Dispose( false );
        }

        public virtual void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( Boolean disposing )
        {
            if( disposing )
            {
                ShutDown( true );

                if( m_cancellationSource != null )
                {
                    m_cancellationSource.Cancel();
                    m_cancellationSource.Dispose();
                    m_cancellationSource = null;
                }
            }
        }

        public static ProcessStartInfo CreateNodeProcessStartInfo<T>( string applicationName, string nodeName )
            where T : INodeFactory
        {
            Preconditions.CheckNotBlank( applicationName, "applicationName" );
            Preconditions.CheckNotBlank( nodeName, "nodeName" );

            return CreateNodeProcessStartInfo( typeof( T ).AssemblyQualifiedName, applicationName, nodeName );
        }

        public static ProcessStartInfo CreateNodeProcessStartInfo( string type, string applicationName, string nodeName )
        {
            Preconditions.CheckNotBlank( type, "type" );
            Preconditions.CheckNotBlank( applicationName, "applicationName" );
            Preconditions.CheckNotBlank( nodeName, "nodeName" );

            var path = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            path = Path.Combine( path, "NetTp.AppRunnerStub.exe" );

            var psi = new ProcessStartInfo(
                path,
                string.Format(
                    "\"{0}\" \"{1}\" \"{2}\" \"{3}\"",
                    typeof( RunnableNode ).FullName,
                    type,
                    applicationName,
                    nodeName ) );

            return psi;
        }

        public static Process StartNodeProcess( string type, string applicationName, string nodeName )
        {
            Preconditions.CheckNotBlank( applicationName, "applicationName" );
            Preconditions.CheckNotBlank( nodeName, "nodeName" );

            return Process.Start( CreateNodeProcessStartInfo( type, applicationName, nodeName ) );
        }

        public static Process StartNodeProcess<T>( string applicationName, string nodeName )
            where T : INodeFactory
        {
            Preconditions.CheckNotBlank( applicationName, "applicationName" );
            Preconditions.CheckNotBlank( nodeName, "nodeName" );

            return Process.Start( CreateNodeProcessStartInfo<T>( applicationName, nodeName ) );
        }

        public static Process GetOrStartApplication( string applicationName )
        {
            Preconditions.CheckNotBlank( applicationName, "applicationName" );

            var nodeFinder = ObjectFactory.GetInstance<INodeFinder>();
            var nodeProcess = nodeFinder.FindNodeProcessByNodeName( applicationName, "ApplicationNode" );

            if( nodeProcess != null )
            {
                Console.WriteLine( "Existing node found {0} - {1}", applicationName, nodeProcess.Id );
                return nodeProcess;
            }

            return StartNodeProcess<ApplicationNode>( applicationName, "ApplicationNode" );
        }

        public static string MakeNodeHeartbeatName( string applicationName, string nodeName )
        {
            Preconditions.CheckNotBlank( applicationName, "applicationName" );
            Preconditions.CheckNotBlank( nodeName, "nodeName" );

            var env = ObjectFactory.GetInstance<IEnvironment>();
            return MakeNodeHeartbeatName( applicationName, nodeName, env.MachineName );
        }

        public static string MakeNodeHeartbeatName( string applicationName, string nodeName, string machineName )
        {
            Preconditions.CheckNotBlank( applicationName, "applicationName" );
            Preconditions.CheckNotBlank( nodeName, "nodeName" );
            Preconditions.CheckNotBlank( machineName, "machineName" );

            return string.Format( "NetTp.grid.heartbeat.{0}:{1}:{2}", machineName, applicationName, nodeName );
        }
    }
}
