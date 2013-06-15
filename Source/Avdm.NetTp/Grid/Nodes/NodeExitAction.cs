namespace Avdm.NetTp.Grid.Nodes
{
    public enum NodeExitAction
    {
        /// <summary>
        /// Take no action. The child is allowed to exit
        /// </summary>
        Ignore,

        /// <summary>
        /// Restart the child
        /// </summary>
        Restart,

        /// <summary>
        /// The child should not be restarted. The supervising node must fail
        /// </summary>
        Fail
    }
}