using System;
using System.Diagnostics;
using System.Threading;
using Avdm.Core;
using Avdm.Core.Logging;
using Avdm.NetTp.Grid.NodeResponsibilityHandlers;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Messaging;
using StructureMap;

namespace Avdm.NetTp.Grid.Executors
{
    /// <summary>
    /// Executor for running a node in a seperate process
    /// </summary>
    /// <remarks>
    /// This is a simple wrapper around a ProcessExecutor with node specific semantics
    /// </remarks>
    public class NodeProcessExecutor<TNodeFactory> : IExecutor
            where TNodeFactory : INodeFactory, new()
    {
        private readonly ProcessExecutor m_processExecutor;

        public NodeProcessExecutor(
            string applicationName,
            string nodeName,
            Process existingProcess,
            int? existingPid )
        {
            Preconditions.CheckNotBlank( applicationName, "applicationName" );
            Preconditions.CheckNotBlank( nodeName, "nodeName" );

            NodeName = nodeName;
            ApplicationName = applicationName;

            m_processExecutor = new ProcessExecutor(
                applicationName + "." + nodeName,
                existingProcess,
                existingPid,
                Node.CreateNodeProcessStartInfo<TNodeFactory>( applicationName, nodeName ),
                FindNodeProcess );

            m_processExecutor.Exited += ( o, e ) => Exited( this, e );
        }

        private Process FindNodeProcess()
        {
            var nodeFinder = ObjectFactory.GetInstance<INodeFinder>();
            return nodeFinder.FindNodeProcessByNodeName( ApplicationName, NodeName );
        }

        public event ExecutorExitedHandler Exited = delegate { };

        public Guid Id
        {
            get { return m_processExecutor.Id; }
        }

        public string Type
        {
            get { return m_processExecutor.Type; }
        }

        public object ChildId
        {
            get { return m_processExecutor.ChildId; }
        }

        public string ChildName
        {
            get { return m_processExecutor.ChildName; }
        }

        public bool IsExecuting
        {
            get { return m_processExecutor.IsExecuting; }
        }

        public int? Pid
        {
            get { return m_processExecutor.Pid; }
        }

        public void Start()
        {
            m_processExecutor.Start();
        }

        public void ShutDown( bool succeeded )
        {
            try
            {
                var nodeFinder = ObjectFactory.GetInstance<INodeFinder>();
                var gid = nodeFinder.FindLocalNodeIdByName( ApplicationName, NodeName );

                if( gid != Guid.Empty )
                {
                    var bus = ObjectFactory.GetInstance<INetTpMessageBus>();
                    bus.PublishEvent( new RemoteShutdownNodeEventMessage( gid, succeeded ) );

                    //TODO sleeping to give not chance to shutdown, not good.... Either do a RPC or wait for confirmation
                    Thread.Sleep( 1000 );
                }

                m_processExecutor.ShutDown( succeeded );
            }
            catch( Exception ex )
            {
                Log.Error( string.Format( "NodeProcessExecutor.Shutdown: app={0}, node={1}", ApplicationName, NodeName ), ex );
                throw;
            }
        }

        public void Dispose()
        {
            m_processExecutor.Dispose();
        }

        public string NodeName { get; private set; }
        public string ApplicationName { get; set; }
    }
}
