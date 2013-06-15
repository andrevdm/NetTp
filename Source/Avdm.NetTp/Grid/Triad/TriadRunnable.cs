using System;
using System.Diagnostics;
using System.Threading;
using Avdm.Core.Logging;
using Avdm.Core.Console;
using Avdm.NetTp.Core;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Messaging;
using StructureMap;

namespace Avdm.NetTp.Grid.Triad
{
    /// <summary>
    /// Starts a triad
    /// 
    /// The triad consists of the triad primary, triad secondary and the application node
    /// Each runs in its own process. The supervision tree looks like this
    /// 
    ///          +-------------------+           
    ///          |                   |           
    ///          V                   |           
    ///    [triad primary]           |           
    ///          |            [triad secondary]  
    ///          |                   ^           
    ///          V                   |           
    ///   [application node]---------+           
    ///          |                               
    ///          V                               
    /// 
    /// This triad of node (processes) ensures that the process graph is kept in a robust a fashion as possible
    /// </summary>
    public class TriadRunnable : IRunnable
    {
        public int Run( string[] args )
        {
            StandardInitialiser.Configure();

            var bus = ObjectFactory.GetInstance<INetTpMessageBus>();

            if( args.Length != 1 )
            {
                Console.WriteLine( "Unknown parameters" );
                Console.WriteLine( "   param 1 = application name" );
                bus.PublishEvent( NodeLoggingEventMessage.Error( null, "Unhandled exception: " + Process.GetCurrentProcess().ProcessName ) );
                return 1;
            }

            string applicationName = args[0];

            try
            {
                var primaryFactory = new TriadPrimaryNode();
                var primaryNode = primaryFactory.Create( applicationName, "Triad primary" );

                var wait = new ManualResetEvent( false );

                primaryNode.NodeEnded += ( o, e ) => wait.Set();
                wait.WaitOne();
                return 0;
            }
            catch( Exception ex )
            {
                Log.Error( string.Format( "TriadRunnable: {0}", applicationName ), ex );
                return 1;
            }
        }
    }
}