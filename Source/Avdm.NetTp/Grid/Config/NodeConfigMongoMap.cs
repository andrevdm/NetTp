using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Avdm.NetTp.Grid.Config
{
    public static class NodeConfigMongoMap
    {
        public static void Setup()
        {
            if( !MongoDB.Bson.Serialization.BsonClassMap.IsClassMapRegistered( typeof( NodeConfig ) ) )
            {
                MongoDB.Bson.Serialization.BsonClassMap.RegisterClassMap<PermanentNodeSupervisionStrategy>( cm => cm.AutoMap() );
                MongoDB.Bson.Serialization.BsonClassMap.RegisterClassMap<TemporaryNodeSupervisionStrategy>( cm => cm.AutoMap() );
                MongoDB.Bson.Serialization.BsonClassMap.RegisterClassMap<ImortalNodeSupervisionStrategy>( cm => cm.AutoMap() );
                MongoDB.Bson.Serialization.BsonClassMap.RegisterClassMap<TransientNodeSupervisionStrategy>( cm => cm.AutoMap() );

                MongoDB.Bson.Serialization.BsonClassMap.RegisterClassMap<OneForOneNodeRestartStrategy>( cm => cm.AutoMap() );
                MongoDB.Bson.Serialization.BsonClassMap.RegisterClassMap<OneForAllNodeRestartStrategy>( cm => cm.AutoMap() );
                MongoDB.Bson.Serialization.BsonClassMap.RegisterClassMap<RestForOneNodeRestartStrategy>( cm => cm.AutoMap() );
                
                MongoDB.Bson.Serialization.BsonClassMap.RegisterClassMap<NodeConfig>( cm =>
                {
                    cm.AutoMap();
                    cm.SetIdMember( cm.GetMemberMap( c => c.Name ) );
                    cm.IdMemberMap.SetIdGenerator( NullIdChecker.Instance );
                    cm.GetMemberMap( c => c.ConfigId ).SetRepresentation( BsonType.String );
                    cm.GetMemberMap( c => c.WorkerStrategy ).SetRepresentation( BsonType.String );
                } );
            }
        }
    }
}
