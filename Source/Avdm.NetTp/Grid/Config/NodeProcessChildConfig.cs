using System;

namespace Avdm.NetTp.Grid.Config
{
    /// <summary>
    /// Stores the configuration for a processes supervised by a node.
    /// See NodeConfig
    /// </summary>
    [Serializable]
    public class NodeProcessChildConfig
    {
        /// <summary>
        /// Descriptive nave for the process. NB this is not use for the file name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The full path to the executable to run
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The working directory
        /// </summary>
        public string WorkingDirectory { get; set; }
    }
}