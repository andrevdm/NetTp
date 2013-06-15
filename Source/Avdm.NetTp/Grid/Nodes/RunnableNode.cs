using System;
using System.Diagnostics;
using Avdm.Core.Console;
using Avdm.Core.Logging;
using Avdm.NetTp.Messaging;
using StructureMap;

namespace Avdm.NetTp.Grid.Nodes
{
    public class RunnableNode : IRunnable
    {
        public int Run( string[] args )
        {
            var bus = ObjectFactory.GetInstance<INetTpMessageBus>();

            if( args.Length < 3 )
            {
                Console.WriteLine( "Unknown parameters" );
                Console.WriteLine( "   param 1 = node creator type (must implement INodeFactory)" );
                Console.WriteLine( "   param 2 = application name" );
                Console.WriteLine( "   param 3 = node name" );
                Console.WriteLine( "Got" );
                Console.WriteLine( "   " + string.Join( " ", args ) );

                bus.PublishEvent( NodeLoggingEventMessage.Error( null, "Error starting node - invalid parameters: " + string.Join( " ", args ) ) );

                return 1;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            try
            {
                string factoryTypeName = args[0];
                string applicationName = args[1];
                string nodeName = args[2];

                Console.Title = string.Format( "{0}. app='{1}'", nodeName, applicationName );

                var factoryType = Type.GetType( factoryTypeName, true );
                var factory = (INodeFactory)Activator.CreateInstance( factoryType );
                var node = factory.Create( applicationName, nodeName );

                var result = 2;

                node.NodeEnded += ( o, e ) =>
                    {
                        result = e.Succeeded ? 0 : 3;
                    };

                Console.CancelKeyPress += ( o, c ) =>
                {
                    Console.WriteLine( "Ctrl-c presses, shutting down" );
                    node.ShutDown( true );
                };

                node.CancellationToken.WaitHandle.WaitOne();
                bus.PublishEvent( NodeLoggingEventMessage.ProcessSucceeded( node, Process.GetCurrentProcess().ProcessName ) );

                return result;
            }
            catch( Exception ex )
            {
                string applicationName = args.Length > 1 ? args[1] : "?";
                string nodeName = args.Length > 2 ? args[2] : "?";

                Log.Error( string.Format( "RunnableNode: App={0}, node={1}", applicationName, nodeName ), ex );

                bus.PublishEvent( NodeLoggingEventMessage.ProcessFailed( null, ex, "Process failed: " + Process.GetCurrentProcess().ProcessName + " - " + ex.ToString() ) );

                return 1;
            }
        }

        private void CurrentDomainUnhandledException( object sender, UnhandledExceptionEventArgs e )
        {
            Log.Error( "Unhandled exception: " + Process.GetCurrentProcess().ProcessName + "\r\n" + e.ExceptionObject );
            Console.WriteLine( e.ExceptionObject );

            var bus = ObjectFactory.GetInstance<INetTpMessageBus>();
            bus.PublishEvent( NodeLoggingEventMessage.Error( null, e.ExceptionObject, "Unhandled exception: " + Process.GetCurrentProcess().ProcessName ) );
        }
    }
}
