using System;
using System.Collections.Generic;

namespace Avdm.NetTp.Core
{
    /// <summary>
    /// Implemented by classes that store a history of processes execution.
    /// E.g. can be used so that process monitors know what processes were previously started
    /// </summary>
    public interface IProcessHistory
    {
        /// <summary>
        /// Record that a processes terminated
        /// </summary>
        /// <param name="pid">Process Id</param>
        /// <param name="machineName">Machine name</param>
        /// <param name="ownerName">Owner</param>
        void ProcessClosed( int pid, string machineName, string ownerName );

        /// <summary>
        /// Record that a processes started
        /// </summary>
        /// <param name="pid">Process Id</param>
        /// <param name="processName"></param>
        /// <param name="machineName">Machine name</param>
        /// <param name="ownerName">Owner</param>
        void ProcessStarted( int pid, string processName, string machineName, string ownerName );

        /// <summary>
        /// Return all started processes on a machine for an owner
        /// </summary>
        /// <param name="machineName">Machine name</param>
        /// <param name="ownerName">Owner</param>
        /// <returns></returns>
        IEnumerable<Tuple<int,string>> GetStartedProcesses( string machineName, string ownerName );
    }
}