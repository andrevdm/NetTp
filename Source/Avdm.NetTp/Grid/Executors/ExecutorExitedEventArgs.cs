using System;

namespace Avdm.NetTp.Grid.Executors
{
    public class ExecutorExitedEventArgs : EventArgs
    {
        static ExecutorExitedEventArgs()
        {
            Fail = new ExecutorExitedEventArgs( false );
            Success = new ExecutorExitedEventArgs( true );
        }

        public ExecutorExitedEventArgs( bool succeeded, Exception ex = null )
        {
            Exception = ex;
            Succeeded = succeeded;
        }

        public bool Succeeded { get; private set; }
        public Exception Exception { get; private set; }

        public static ExecutorExitedEventArgs Success { get; private set; }
        public static ExecutorExitedEventArgs Fail { get; private set; }
    }
}