using System;
using System.Diagnostics;
using Avdm.Core;
using Avdm.NetTp.Core;
using StructureMap;

namespace Avdm.NetTp.Grid.Executors
{
    /// <summary>
    /// Executor for running processes
    /// </summary>
    public class ProcessExecutor : IExecutor
    {
        private readonly string m_name;
        private readonly ProcessStartInfo m_startInfo;
        private readonly Func<Process> m_processFinder;
        private readonly object m_sync = new object();
        private readonly IProcessHistory m_processHistory;
        private Process m_process;

        public event ExecutorExitedHandler Exited;
        public Guid Id { get; private set; }
        public string Type { get { return "Process"; } }
        public object ChildId { get { return m_process != null ? m_process.Id : -1; } }
        public string ChildName { get { return m_process != null ? m_process.ProcessName : ""; } }

        public ProcessExecutor(
            string name,
            Process existingProcess,
            int? existingPid,
            ProcessStartInfo startInfo,
            Func<Process> processFinder )
        {
            Preconditions.CheckNotNull( startInfo, "startInfo" );

            m_processHistory = ObjectFactory.GetInstance<IProcessHistory>();

            Id = Guid.NewGuid();
            m_name = name;
            m_startInfo = startInfo;
            m_processFinder = processFinder;

            if( existingProcess != null )
            {
                m_process = existingProcess;
                m_process.EnableRaisingEvents = true;
                m_process.Exited += ProcessExited;
                m_processHistory.ProcessStarted( m_process.Id, m_process.ProcessName, Environment.MachineName, m_name );

                Console.WriteLine( "exec.ctor {0} using existing process {1}", m_name, existingProcess.Id );
            }
            else
            {
                if( existingPid != null )
                {
                    try
                    {
                        m_process = Process.GetProcessById( existingPid.Value );
                        m_process.EnableRaisingEvents = true;
                        m_process.Exited += ProcessExited;
                        m_processHistory.ProcessStarted( m_process.Id, m_process.ProcessName, Environment.MachineName, m_name );

                        Console.WriteLine( "exec.ctor {0} using existing pid {1}", m_name, existingProcess.Id );
                    }
                    catch
                    {
                        m_process = null;
                    }
                }
            }
        }

        public bool IsExecuting { get { return m_process != null && !m_process.HasExited; } }
        public int? Pid { get { return IsExecuting ? m_process.Id : (int?)null; } }

        public void Start()
        {
            Console.WriteLine( "exec starting" );

            lock( m_sync )
            {
                Process existingProcess = m_process;
                int? existingPid = existingProcess != null && !existingProcess.HasExited ? existingProcess.Id : (int?)null;

                if( existingProcess != null )
                {
                    Console.WriteLine( "exec {0} using existing process {1}", m_name, existingProcess.Id );
                    m_process = existingProcess;
                }

                if( (m_process == null) && (m_processFinder != null) )
                {
                    m_process = m_processFinder();
                    Console.WriteLine( "exec {0} using process finder, found={1}", m_name, m_process == null );

                    if( m_process == null )
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine( "Process not found using process finder " + m_name + " - " + m_processFinder );
                        Console.ResetColor();
                    }
                }

                if( m_process != null )
                {
                    Console.WriteLine( "exec {0} using process {1}", m_name, m_process.Id );

                    existingPid = m_process.Id;
                    m_process.EnableRaisingEvents = true;
                    m_process.Exited += ProcessExited;
                    m_processHistory.ProcessStarted( m_process.Id, m_process.ProcessName, Environment.MachineName, m_name );
                }

                foreach( var old in m_processHistory.GetStartedProcesses( Environment.MachineName, m_name ) )
                {
                    try
                    {
                        var oldProcess = Process.GetProcessById( old.Item1 );

                        if( (oldProcess != null) && (!oldProcess.HasExited) && (oldProcess.ProcessName == old.Item2) )
                        {
                            Console.WriteLine( "Old process exists using pid={0} name={1}", oldProcess.Id, old.Item2 );

                            m_process = oldProcess;
                            m_process.EnableRaisingEvents = true;
                            m_process.Exited += ProcessExited;
                            m_processHistory.ProcessStarted( m_process.Id, m_process.ProcessName, Environment.MachineName, m_name );
                        }
                        else
                        {
                            if( (existingPid != oldProcess.Id) && (oldProcess.ProcessName == old.Item2) )
                            {
                                try
                                {
                                    Console.WriteLine( "exec {0} killing {1}", m_name, oldProcess.Id );
                                    oldProcess.Kill();
                                }
                                catch
                                {
                                }
                            }
                            else
                            {
                                Console.WriteLine( "exec {0} not killing existing {1}", m_name, existingPid );
                            }

                            m_processHistory.ProcessClosed( old.Item1, Environment.MachineName, m_name );
                        }
                    }
                    catch
                    {
                        m_processHistory.ProcessClosed( old.Item1, Environment.MachineName, m_name );
                    }
                }

                if( m_process == null )
                {
                    if( m_process == null )
                    {
                        Console.WriteLine( "exec {0} starting new process", m_name );
                        m_process = Process.Start( m_startInfo );
                    }

                    m_process.EnableRaisingEvents = true;
                    m_process.Exited += ProcessExited;
                    m_processHistory.ProcessStarted( m_process.Id, m_process.ProcessName, Environment.MachineName, m_name );
                }
            }
        }

        public void ShutDown( bool succeeded )
        {
            if( m_process != null )
            {
                int id = m_process.Id;

                m_process.Exited -= ProcessExited;

                if( !m_process.HasExited )
                {
                    m_process.WaitForExit( 800 );

                    if( m_process != null )
                    {
                        try
                        {
                            m_process.Kill();
                        }
                        catch( InvalidOperationException ex )
                        {
                        }
                    }

                    m_processHistory.ProcessClosed( id, Environment.MachineName, m_name );
                }

                m_process = null;
            }
        }

        private void ProcessExited( object sender, EventArgs eventArgs )
        {
            bool success = true;

            if( m_process != null )
            {
                m_process.Exited -= ProcessExited;
                success = m_process.ExitCode == 0;
                m_processHistory.ProcessClosed( m_process.Id, Environment.MachineName, m_name );
                m_process = null;
            }

            Exited( this, new ExecutorExitedEventArgs( success ) );
        }

        ~ProcessExecutor()
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

                if( m_process != null )
                {
                    m_process.Dispose();
                    m_process = null;
                }
            }
        }
    }
}