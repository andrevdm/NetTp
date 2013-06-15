using Avdm.Config;
using Avdm.Core;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Avdm.NetTp.Grid.Config
{
    public class NodeConfigPersistor : INodeConfigPersistor
    {
        public NodeConfig LoadAppConfig( string applicationName )
        {
            Preconditions.CheckNotNull( applicationName, "applicationName" );

            var coll = GetCollection();
            var config = coll.FindOneAs<NodeConfig>( Query.EQ( "_id", applicationName ) );
            return config;
        }

        public void SaveAppConfig( NodeConfig config )
        {
            Preconditions.CheckNotNull( config, "config" );

            //An application is always a process
            config.IsProcess = true;

            var coll = GetCollection();
            coll.Update( 
                Query.EQ( "_id", config.Name ),
                Update.Replace( config ),
                UpdateFlags.Upsert,
                WriteConcern.Acknowledged );
        }

        private MongoCollection GetCollection()
        {
            var client = new MongoClient( ConfigManager.AppSettings["MongoDB.Server"] );
            var server = client.GetServer();
            var db = server.GetDatabase( "NetTpGrid" );

            if( !db.CollectionExists( "AppConfig" ) )
            {
                db.CreateCollection( "AppConfig" );
                var coll = db.GetCollection( "AppConfig" );
                coll.EnsureIndex( IndexKeys.Ascending( "Name" ), IndexOptions.SetUnique( true ) );
            }

            return db.GetCollection( "AppConfig" );            
        }
    }
}