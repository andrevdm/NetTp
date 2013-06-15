using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Avdm.Config;
using Avdm.Core.TestApp;
using Avdm.Deploy.Sbin;
using Avdm.NetTp.Core;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.Pool;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;
using Avdm.NetTp.Messaging;
using StructureMap;

namespace Avdm.NetTp.Tester
{
    public class TesterProgram : ConsoleTestApp
    {
        private bool m_sbinUpdaterRunning = false;

        static void Main( string[] args )
        {
            StandardInitialiser.Configure();

            new TesterProgram().Start();
        }

        public TesterProgram()
        {
            MenuItems.Add( new MenuItem( "Grid", "Inproc proc node", InProcNode ) );
            MenuItems.Add( new MenuItem( "Grid", "Inproc proc Supervisor tree - Nodes with process executor", TestProcessExcutor ) );
            MenuItems.Add( new MenuItem( "Grid", "Out of proc Supervisor tree - Nodes with process executor", TestOutOfProcProcessExcutor ) );
            MenuItems.Add( new MenuItem( "Grid", "Remote shutdown", RemoteShutDown ) );
            MenuItems.Add( new MenuItem( "Grid", "Remote shutdown everything", RemoteShutDownEverything ) );
            MenuItems.Add( new MenuItem( "Grid", "Start an application triad - App1", StartTriadApp1 ) );
            MenuItems.Add( new MenuItem( "Grid", "Test hot swapping", TestHotSwapping ) );
            MenuItems.Add( new MenuItem( "Grid", "Send dummy command", SendDummyCommand ) );
            MenuItems.Add( new MenuItem( "Runner", "Run demo", TestAppRunnerStub ) );
            MenuItems.Add( new MenuItem( "Runner", "Run node created by RunnableNode", TestRunnableNode ) );
            MenuItems.Add( new MenuItem( "sbin", "Get sbin info", GetSbinInfo ) );
            MenuItems.Add( new MenuItem( "sbin", "Start sbin updater", SbinUpdater ) );
            MenuItems.Add( new MenuItem( "Bus", "Test cross-process RPC", OutOfProcRpcTest ) );

            SbinUpdater();
        }

        private void InProcNode()
        {
            var nodeA = new Node( "TestApp", "in proc test", NodeWorkerStrategy.Supervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultTemporary );
            nodeA.StartWorker( ( n, c ) => Thread.Sleep( 1000 ) );
        }

        private void TestProcessExcutor()
        {
            Console.WriteLine( "Starting supervisor tree" );
            Console.WriteLine( "                         +----> notepad.exe" );
            Console.WriteLine( "                         |" );
            Console.WriteLine( "        +----> (B)-------+ " );
            Console.WriteLine( "        |                |" );
            Console.WriteLine( "        |                +----> winver.exe" );
            Console.WriteLine( "        |" );
            Console.WriteLine( "  (A)---+" );
            Console.WriteLine( "        |" );
            Console.WriteLine( "        +----> calc.exe" );
            Console.WriteLine( "        |" );
            Console.WriteLine( "        +----> (C)-------> ftp.exe" );
            Console.WriteLine();
            Console.WriteLine( "  Node A: OneForOne, Imortal" );
            Console.WriteLine( "  Node B: OneForAll, Permanent - max 3 restarts in 2 minutes" );
            Console.WriteLine( "  Node C: OneForOne, Transient - close = falied run, type 'quit' for a succesfull run" );
            Console.WriteLine();

            var nodeA = new Node( "TestApp", "nodeA", NodeWorkerStrategy.DontSupervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultImortal );

            try
            {
                var psi = new ProcessStartInfo { FileName = @"C:\Windows\System32\calc.exe" };
                var executorA1 = new ProcessExecutor( "TestApp:execA", null, null, psi, null );
                executorA1.Exited += ( o, e ) => Console.WriteLine( "calc exited, succeeded={0}", e.Succeeded );
                nodeA.Supervise( executorA1 );

                int nodeBCount = 0;

                var nodeBExecutor = new NodeExecutor(
                    () =>
                    {
                        Console.WriteLine( "Restarting node B {0}", ++nodeBCount );

                        var nodeB = new Node(
                            "TestApp", 
                            "nodeB",
                            NodeWorkerStrategy.DontSupervise,
                            NodeRestartStrategy.OneForAll,
                            NodeSupervisionStrategy.Permanent( 3, TimeSpan.FromMinutes( 2 ), new[] { 0, 200, 500 } ) );

                        psi = new ProcessStartInfo { FileName = @"C:\Windows\System32\notepad.exe" };
                        var executorB1 = new ProcessExecutor( "TestApp:execB1", null, null, psi, null );
                        executorB1.Exited += ( o, e ) => Console.WriteLine( "notepad executor exited - node B{0}, succeeded={1}", nodeBCount, e.Succeeded );
                        nodeB.Supervise( executorB1 );

                        psi = new ProcessStartInfo { FileName = @"C:\Windows\System32\winver.exe" };
                        var executorB2 = new ProcessExecutor( "TestApp:execB2", null, null, psi, null );
                        executorB2.Exited += ( o, e ) => Console.WriteLine( "winver executor exited - node B{0}, succeeded={1}", nodeBCount, e.Succeeded );
                        nodeB.Supervise( executorB2 );

                        return nodeB;
                    },
                    ( n, c ) => { } );

                int nodeCCount = 0;

                var nodeCExecutor = new NodeExecutor(
                     () =>
                     {
                         Console.WriteLine( "Restarting node C{0}", ++nodeCCount );

                         var nodeC = new Node(
                             "TestApp", 
                             "nodeC",
                             NodeWorkerStrategy.DontSupervise,
                             NodeRestartStrategy.OneForAll,
                             NodeSupervisionStrategy.DefaultTransient );

                         psi = new ProcessStartInfo { FileName = @"C:\Windows\System32\ftp.exe" };
                         var executor = new ProcessExecutor( "TestApp:execC1", null, null, psi, null );
                         executor.Exited += ( o, e ) => Console.WriteLine( "ftp executor exited - node B{0}, succeeded={1}", nodeBCount, e.Succeeded );
                         nodeC.Supervise( executor );

                         return nodeC;
                     },
                     ( n, c ) => { } );


                nodeA.Supervise( nodeBExecutor );
                nodeA.Supervise( nodeCExecutor );
                Console.WriteLine( "Running, press return to stop" );
                Console.ReadLine();
            }
            finally
            {
                nodeA.ShutDown( true );
            }
        }

        private void TestOutOfProcProcessExcutor()
        {
            Console.WriteLine( "Starting supervisor tree" );
            Console.WriteLine( "   each node is in its own process" );
            Console.WriteLine();
            Console.WriteLine( "                         +----> notepad.exe" );
            Console.WriteLine( "                         |" );
            Console.WriteLine( "        +----> (B)-------+ " );
            Console.WriteLine( "        |                |" );
            Console.WriteLine( "        |                +----> winver.exe" );
            Console.WriteLine( "        |" );
            Console.WriteLine( "  (A)---+" );
            Console.WriteLine( "        |" );
            Console.WriteLine( "        +----> calc.exe" );
            Console.WriteLine( "        |" );
            Console.WriteLine( "        +----> (C)-------> ftp.exe" );
            Console.WriteLine();
            Console.WriteLine( "  Node A: OneForOne, Imortal" );
            Console.WriteLine( "  Node B: OneForAll, Permanent - max 3 restarts in 2 minutes" );
            Console.WriteLine( "  Node C: OneForOne, Transient - close = falied run, type 'quit' for a succesfull run" );
            Console.WriteLine();

            Node.StartNodeProcess<TestRunnableNodeOutOfProcNodeA>( "Tester", "TestRunnableNodeOutOfProcNodeA" );
        }


        private void TestAppRunnerStub()
        {
            var path = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            path = Path.Combine( path, "NetTp.AppRunnerStub.exe" );
            Process.Start( path, "\"NetTp.Tester.TestRunnable, NetTp.Tester\" 1 \"param 2\" 3 4" );
        }

        private void TestRunnableNode()
        {
            Node.StartNodeProcess<TestRunnableNode>( "Tester", "TestRunnableNode" );
        }

        private void GetSbinInfo()
        {
            var resolver = ObjectFactory.GetInstance<ISbinAssemblyResolver>();
            Console.WriteLine( "Running in sbin={0}", resolver.IsRunningInSbin );
            Console.WriteLine( "Running in current version={0}", resolver.CurrentVersion );
            Console.WriteLine( "Config value for key 'TestKey'={0}", ConfigManager.AppSettings["TestKey"] );
        }

        private void OutOfProcRpcTest()
        {
            Node.StartNodeProcess<TestOutOfProcRpcA>( "Tester", "OutOfProcRpcTest" );
        }

        private void RemoteShutDown()
        {
            var guid = Guid.Parse( Prompt( "Node GUID" ) );

            var remote = ObjectFactory.GetInstance<INodeRemoteControl>();
            remote.ShutDown( guid );
        }

        private void RemoteShutDownEverything()
        {
            var remote = ObjectFactory.GetInstance<INodeRemoteControl>();
            remote.ShutDownAll( false );
        }

        private void StartTriadApp1()
        {
            Node.GetOrStartApplication( "TestApp1" );
        }

        private void SbinUpdater()
        {
            if( m_sbinUpdaterRunning )
            {
                Console.WriteLine( "Already running" );
                return;
            }

            Console.WriteLine( "Starting sbin updater" );

            m_sbinUpdaterRunning = true;

            var node = new Node( "TestApp", "swap", NodeWorkerStrategy.Supervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultTemporary );
            node.StartWorker( ( n, c ) =>
            {
                var watcher = new SbinAutoUpdateFileWatcherWorker();
                watcher.Run();
            } );
        }

        private void TestHotSwapping()
        {
            var node = new Node( "TestApp", "swap", NodeWorkerStrategy.Supervise, NodeRestartStrategy.OneForOne, NodeSupervisionStrategy.DefaultTemporary );

            node.StartWorker( ( n, c ) =>
                {
                    int count = 0;

                    var pool = new HotSwappableHandlerPool( node, GetType().Assembly.FullName );
                    
                    var bus = ObjectFactory.GetInstance<INetTpMessageBus>();

                    Console.WriteLine( "press q to exit, any other key to send message" );

                    while( !c.IsCancellationRequested )
                    {
                        bus.PublishCommand( new DummyCommand() { Message = (count++).ToString() } );
                        Thread.Sleep( 500 );
                    }
                } );
        }

        private void SendDummyCommand()
        {
            var bus = ObjectFactory.GetInstance<INetTpMessageBus>();
            bus.PublishCommand( new DummyCommand() { Message = DateTime.Now.ToString() } );
        }
    }
}