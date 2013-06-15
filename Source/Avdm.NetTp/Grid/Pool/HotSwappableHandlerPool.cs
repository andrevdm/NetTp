using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avdm.Config;
using Avdm.Core;
using Avdm.Deploy.Sbin;
using Avdm.Core.Logging;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Messaging;
using StructureMap;

namespace Avdm.NetTp.Grid.Pool
{
    public class HotSwappableHandlerPool
    {
        private readonly Node m_node;
        private readonly string m_loaderType;
        private readonly string m_loaderArg;
        private readonly SemaphoreSlim m_semaphore;
        private SwappablePoolInfo m_currentPoolInfo;
        private readonly ConcurrentDictionary<Guid, SwappablePoolInfo> m_oldPools = new ConcurrentDictionary<Guid, SwappablePoolInfo>();
        private readonly Timer m_timer;
        private readonly int m_refreshPeriodSeconds;
        private readonly IHotSwapHandlerStrategy m_hotSwapStrategy;
        private readonly INetTpMessageBus m_messageBus;
        private readonly object m_syncHotSwap = new object();

        public HotSwappableHandlerPool( Node node, string loaderArg )
            : this( node, null, loaderArg )
        {
        }

        public HotSwappableHandlerPool( Node node, string loaderType, string loaderArg )
        {
            Preconditions.CheckNotNull( node, "node" );

            m_node = node;
            m_loaderType = loaderType ?? typeof(ScanLoadedAssembliesForHandlersLoader).FullName;
            m_loaderArg = loaderArg;
            m_semaphore = new SemaphoreSlim( Environment.ProcessorCount * 2 );
            m_messageBus = ObjectFactory.GetInstance<INetTpMessageBus>();

            m_hotSwapStrategy = new SbinUpdateHotSwapHandlerStrategy( m_node, new[] { GetType() } );
            m_hotSwapStrategy.Init( this );

            m_currentPoolInfo = LoadHotSwappableHost();
            m_refreshPeriodSeconds = int.Parse( ConfigManager.AppSettings["Grid.HotSwappableHandlerPool.RetireOldSwappablesPeriodSeconds"] ?? "60" );
            m_timer = new Timer( CheckOldSwappables, null, m_refreshPeriodSeconds, Timeout.Infinite );
        }

        private SwappablePoolInfo LoadHotSwappableHost()
        {
            var setup = new AppDomainSetup();
            setup.ApplicationName = "HotSwappableWorkerHost";
            setup.ApplicationBase = Path.GetDirectoryName( Assembly.GetEntryAssembly().Location );

            var resolver = ObjectFactory.GetInstance<ISbinAssemblyResolver>();
            var type = typeof( HotSwappableHandlers );
            var d = resolver.CreateAndUnwrapAppDomain( "HotSwappableHandlerPoolDomain", setup, type.Assembly.FullName, type.FullName );
            var domain = d.Item1;
            var handlers = (HotSwappableHandlers)d.Item2;

            var commandHandlerTypes = handlers.RegisterHandlers( m_loaderType, m_loaderArg );
            var subscribeMethod = GetType().GetMethod( "SubscribeToCommand", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod );

            foreach( var commandHandlerType in commandHandlerTypes )
            {
                var method = subscribeMethod.MakeGenericMethod( commandHandlerType );
                method.Invoke( this, null );
            }

            var names = handlers.GetLoadedAssemblyNames();
            m_hotSwapStrategy.AddAssembliesToMonitor( names );

            return new SwappablePoolInfo( domain, handlers );
        }

        public void SwapNow()
        {
            lock( m_syncHotSwap )
            {
                var oldSwappable = m_currentPoolInfo;

                m_currentPoolInfo = LoadHotSwappableHost();

                if( oldSwappable != null )
                {
                    m_oldPools[oldSwappable.Id] = oldSwappable;
                }
            }
        }

        /// <summary>
        /// Subscribes to a command.
        /// When the command is recieved it is sent to the current swappable and then dispatched to the handler.
        /// This acts as the unchanging handler for the service bus, it manages the chancing swappables under the hood.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        protected void SubscribeToCommand<TCommand>() where TCommand : INetTpCommandMessage
        {
            Func<TCommand, Task> constraindHandler = cmd =>
            {
                m_semaphore.Wait();

                var currentPoolInfo = m_currentPoolInfo;
                Interlocked.Increment( ref currentPoolInfo.RunningHandlers );

                var task = Task.Factory.StartNew( () =>
                {
                    try
                    {
                        currentPoolInfo.Handlers.DispatchCommand( cmd );
                    }
                    catch( Exception ex )
                    {
                        Log.Error( "Error dispatching command " + typeof( TCommand ), ex );
                        throw;
                    }
                    finally
                    {
                        m_semaphore.Release();
                        Interlocked.Decrement( ref currentPoolInfo.RunningHandlers );
                    }
                } );

                return task;
            };

            m_messageBus.SubscribeToCommandAsync<TCommand>( "", constraindHandler );
        }

        /// <summary>
        /// Check if any of the old swappables have finished processing all messages.
        /// If so unload them
        /// </summary>
        /// <param name="state"></param>
        private void CheckOldSwappables( object state )
        {
            var oldSwappables = new List<SwappablePoolInfo>( m_oldPools.Values );

            try
            {
                foreach( var old in oldSwappables )
                {
                    if( old.RunningHandlers < 1 )
                    {
                        Console.WriteLine( "Unloading {0} - {1}", old.Id, GetType() );

                        SwappablePoolInfo s;

                        if( m_oldPools.TryRemove( old.Id, out s ) )
                        {
                            AppDomain.Unload( s.Domain );
                        }
                    }
                }
            }
            finally
            {
                m_timer.Change( m_refreshPeriodSeconds, Timeout.Infinite );
            }
        }
    }
}
