namespace Avdm.NetTp.Grid.Nodes
{
    /// <summary>
    /// Implemented by classes that can create nodes
    /// </summary>
    public interface INodeFactory
    {
        /// <summary>
        /// Create a new instance of a node
        /// </summary>
        /// <returns>A new node instance</returns>
        Node Create( string applicationName, string nodeName );
    }
}
