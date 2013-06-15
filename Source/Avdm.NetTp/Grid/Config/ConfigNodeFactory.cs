using System;
using Avdm.Core;
using Avdm.Core.Logging;
using Avdm.NetTp.Grid.Nodes;
using StructureMap;

namespace Avdm.NetTp.Grid.Config
{
    /// <summary>
    /// A node factory that loads a node using the settings from config (INodeConfigPersistor)
    /// </summary>
    public class ConfigNodeFactory : INodeFactory
    {
        /// <summary>
        /// Creates a node
        /// </summary>
        /// <param name="applicationName">Application name</param>
        /// <param name="nodeName">The config GUID preceded by a '*' character</param>
        /// <returns></returns>
        public Node Create( string applicationName, string nodeName )
        {
            try
            {
                Preconditions.CheckNotBlank( applicationName, "applicationName" );
                Preconditions.CheckNotBlank( nodeName, "nodeName" );
                Preconditions.CheckTrue( nodeName.StartsWith( "*" ), "nodeName", "Invalid node name. Expecting ID preceded by '*'" );

                string idString = nodeName.Substring( 1 );
                var id = Guid.Parse( idString );

                var configLoader = ObjectFactory.GetInstance<INodeConfigPersistor>();
                var appConfig = configLoader.LoadAppConfig( applicationName );

                if( appConfig == null )
                {
                    throw new InvalidOperationException( string.Format( "No config found for application {0}", applicationName ) );
                }

                var config = FindConfig( appConfig, id );

                if( config == null )
                {
                    throw new InvalidOperationException( string.Format( "No config found for application={0}, id={1}", applicationName, idString ) );
                }

                Console.Title = string.Format( "{0}. app='{1}' type = 'ConfigNodeFactory'", config.Name, applicationName );

                var node = new Node( applicationName, config );
                return node;
            }
            catch( Exception ex )
            {
                Log.Error( "ConfigNodeFactory", ex );
                throw;
            }
        }

        private NodeConfig FindConfig( NodeConfig config, Guid id )
        {
            if( config.ConfigId == id )
            {
                return config;
            }

            if( config.Nodes != null )
            {
                foreach( var child in config.Nodes )
                {
                    var found = FindConfig( child, id );

                    if( found != null )
                    {
                        return found;
                    }
                }
            }

            return null;
        }
    }
}
