using System;
using System.Collections.Generic;
using Avdm.Core.Caching;
using StructureMap;

namespace Avdm.NetTp.Core
{
    public class SharedCacheProcessHistory : IProcessHistory
    {
        private readonly ICache m_cache = ObjectFactory.GetInstance<ICache>();

        public void ProcessClosed( int pid, string machineName, string ownerName )
        {
            var history = GetMachineHistory( machineName, ownerName );
            history.Ids.RemoveAll( i => i.Item1 == pid );
            m_cache[history.Key] = history;
        }

        public void ProcessStarted( int pid, string processName, string machineName, string ownerName )
        {
            var history = GetMachineHistory( machineName, ownerName );
            history.Ids.Add( new Tuple<int, string>( pid, processName ) );
            m_cache[history.Key] = history;
        }

        public IEnumerable<Tuple<int,string>> GetStartedProcesses( string machineName, string ownerName )
        {
            var history = GetMachineHistory( machineName, ownerName );
            return history.Ids;
        }

        private MachineHistory GetMachineHistory( string machineName, string ownerName )
        {
            MachineHistory history = null;
            string key = MachineHistory.MakeKey( machineName, ownerName );

            try
            {
                history = (MachineHistory)m_cache[key];
            }
            catch( Exception )
            {
            }

            if( history == null )
            {
                history = new MachineHistory( machineName, ownerName );
            }
            return history;
        }

        [Serializable]
        private class MachineHistory
        {
            public MachineHistory( string machineName, string ownerName )
            {
                Key = MakeKey( machineName, ownerName );
                Ids = new List<Tuple<int, string>>();
                MachineName = machineName;
                OwnerName = ownerName;
            }

            public string Key { get; set; }
            public string MachineName { get; set; }
            public string OwnerName { get; set; }
            public List<Tuple<int, string>> Ids { get; set; }

            public static string MakeKey( string machineName, string ownerName )
            {
                return string.Format( "{0}!{1}", machineName, ownerName );
            }
        }
    }
}
