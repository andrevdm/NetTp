using System;
using System.Collections.Generic;
using System.IO;
using Avdm.Core;
using Avdm.NetTp.Core;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Messaging;
using StructureMap;

namespace Avdm.NetTp.Grid.Pool
{
    public class SbinUpdateHotSwapHandlerStrategy : IHotSwapHandlerStrategy
    {
        private readonly Node m_node;
        private readonly INetTpMessageBus m_messageBus;
        private HotSwappableHandlerPool m_parent;
        private readonly Dictionary<string, bool> m_assembliesToMonitor = new Dictionary<string, bool>();

        public SbinUpdateHotSwapHandlerStrategy( Node node, IEnumerable<Type> typesToMonitorForUpdate )
        {
            Preconditions.CheckNotNull( node, "node" );
            Preconditions.CheckNotNull( typesToMonitorForUpdate, "typesToMonitorForUpdate" );

            m_node = node;
            m_messageBus = ObjectFactory.GetInstance<INetTpMessageBus>();

            m_messageBus.SubscribeToEvent<SbinFilesUpdatedEventMessage>( string.Format( "{0}.{1}:SbinUpdateHotSwapHandlerStrategy", node.ApplicationName, node.NodeName ), SbinUpdated );

            m_assembliesToMonitor[GetType().Assembly.FullName.ToLower()] = true;
            m_assembliesToMonitor[typeof( HotSwappableHandlers ).Assembly.FullName.ToLower()] = true;
        }

        public void Init( HotSwappableHandlerPool parent )
        {
            Preconditions.CheckNotNull( parent, "parent" );

            m_parent = parent;
        }

        public void AddAssembliesToMonitor( IEnumerable<string> assemblyNames )
        {
            Preconditions.CheckNotNull( assemblyNames, "assemblyNames" );

            foreach( var fullName in assemblyNames )
            {
                m_assembliesToMonitor[GetAsmNameOnly( fullName )] = true;
            }
        }

        private string GetAsmNameOnly( string fullName )
        {
            var asmName = fullName.ToLower();
            var idx = asmName.IndexOf( "," );
            if( idx > 0 )
            {
                asmName = asmName.Substring( 0, idx ).Trim();
            }

            return asmName;
        }

        /// <summary>
        /// sbin was updated. Check if any of the changes are for the loaded swappable worker
        /// </summary>
        /// <param name="msg"></param>
        private void SbinUpdated( SbinFilesUpdatedEventMessage msg )
        {
            Preconditions.CheckNotNull( msg, "msg" );

            if( m_parent == null )
            {
                return;
            }

            foreach( var asmName in m_assembliesToMonitor.Keys )
            {
                if( msg.FileNames.Exists( f =>
                    {
                        var name = GetAsmNameOnly( Path.GetFileName( f ) );
                        return name == asmName + ".exe" || name == asmName + ".dll";
                    } ) )
                {
                    Console.WriteLine( "HotSwap: change found '{0}'", asmName );
                    m_parent.SwapNow();
                    return;
                }
            }
        }
    }
}