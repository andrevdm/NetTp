namespace Avdm.NetTp.Grid.Executors
{
    public interface IExecutor : IExecutorInfo
    {
        bool IsExecuting { get; }
        void Start();
        void ShutDown( bool succeeded );
        event ExecutorExitedHandler Exited;
    }

    public delegate void ExecutorExitedHandler( object sender, ExecutorExitedEventArgs e );
}