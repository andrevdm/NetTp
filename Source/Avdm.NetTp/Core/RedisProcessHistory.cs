using System;
using System.Collections.Generic;
using System.Linq;
using Sider;
using StructureMap;

namespace Avdm.NetTp.Core
{
    public class RedisProcessHistory : IProcessHistory
    {
        private string FormatKey( string machineName, string ownerName )
        {
            return string.Format( "NetTp.grid.processHistory:{0}.{1}", machineName, ownerName );
        }

        public void ProcessClosed( int pid, string machineName, string ownerName )
        {
            var client = ObjectFactory.GetInstance<IRedisClient<string>>();
            client.HDel( FormatKey( machineName, ownerName ), pid.ToString() );
        }

        public void ProcessStarted( int pid, string processName, string machineName, string ownerName )
        {
            var client = ObjectFactory.GetInstance<IRedisClient<string>>();
            client.HSet( FormatKey( machineName, ownerName ), pid.ToString(), processName );
        }

        public IEnumerable<Tuple<int, string>> GetStartedProcesses( string machineName, string ownerName )
        {
            var client = ObjectFactory.GetInstance<IRedisClient<string>>();

            return (from kv in client.HGetAll( FormatKey( machineName, ownerName ) )
                    select new Tuple<int, string>( int.Parse( kv.Key ), kv.Value ));
        }
    }
}
