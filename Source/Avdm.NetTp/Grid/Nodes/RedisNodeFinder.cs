using System;
using System.Diagnostics;
using Avdm.Core;
using Avdm.NetTp.Core;
using Sider;
using StructureMap;

namespace Avdm.NetTp.Grid.Nodes
{
    public class RedisNodeFinder : INodeFinder
    {
        public Process FindNodeProcessByNodeName( string applicationName, string nodeName )
        {
            Preconditions.CheckNotBlank( applicationName, "applicationName" );
            Preconditions.CheckNotBlank( nodeName, "nodeName" );

            string key = Node.MakeNodeHeartbeatName( applicationName, nodeName );

            Console.WriteLine( "RedisNodeFinder: Searching for app={0}, node={1} ({2})", applicationName, nodeName, key );

            var redis = ObjectFactory.GetInstance<IRedisClient<string>>();
            string pidStr = redis.HGet( key, "pid" );
            int pid;

            if( !int.TryParse( pidStr, out pid ) )
            {
                Console.WriteLine( "RedisNodeFinder: No existing node found for app={0}, node={1} ({2})", applicationName, nodeName, key );
                return null;
            }

            try
            {
                var process = Process.GetProcessById( pid );

                if( !process.HasExited )
                {
                    Console.WriteLine( "RedisNodeFinder: Existing node found for app={0}, node={1} ({2}). Pid={3}", applicationName, nodeName, key, process.Id );
                    return process;
                }
                else
                {
                    Console.WriteLine( "RedisNodeFinder: Existing process node found for app={0}, node={1} ({2}). Pid={3}", applicationName, nodeName, key, process.Id );
                    return null;
                }
            }
            catch( Exception ex )
            {
                Console.WriteLine( "RedisNodeFinder: Exception finding process node for app={0}, node={1} ({2})\r\n{3}", applicationName, nodeName, key, ex );
                return null;
            }
        }

        public Guid FindLocalNodeIdByName( string applicationName, string nodeName )
        {
            Preconditions.CheckNotBlank( applicationName, "applicationName" );
            Preconditions.CheckNotBlank( nodeName, "nodeName" );

            string key = Node.MakeNodeHeartbeatName( applicationName, nodeName );

            Console.WriteLine( "RedisNodeFinder: Searching for app={0}, node={1} ({2})", applicationName, nodeName, key );

            var redis = ObjectFactory.GetInstance<IRedisClient<string>>();
            string gidStr = redis.HGet( key, "gid" );
            Guid gid;

            if( !Guid.TryParse( gidStr, out gid ) )
            {
                Console.WriteLine( "RedisNodeFinder: No existing node gid found for app={0}, node={1} ({2})", applicationName, nodeName, key );
                return Guid.NewGuid();
            }

            return gid;
        }
    }
}