namespace Avdm.NetTp.Grid.Nodes
{
    /// <summary>
    /// Worker strategy indicates if a node will monitor its worker
    /// </summary>
    public enum NodeWorkerStrategy
    {
        /// <summary>
        /// The nodes worker will not supervised. If the worker exists the node will be shutdown
        /// </summary>
        Supervise,

        /// <summary>
        /// The nodes worker will not be supervised. When the worker exits either sucesfully or unsucesfully the node will continue running
        /// </summary>
        DontSupervise
    }
}
