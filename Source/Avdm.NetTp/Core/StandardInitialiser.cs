using Avdm.Config;
using Avdm.Core.Di;
using Avdm.Deploy.Manager;
using Avdm.Deploy.Sbin;
using Avdm.NetTp.Grid.Config;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Messaging;
using EasyNetQ;
using Sider;
using StructureMap;

namespace Avdm.NetTp.Core
{
    public static class StandardInitialiser
    {
        public static void Configure()
        {
            ConfigStructureMap();
            ConfigMongo();
        }

        private static void ConfigMongo()
        {
            NodeConfigMongoMap.Setup();
        }

        private static void ConfigStructureMap()
        {
            ObjectFactory.Configure(
                x =>
                    {
                        x.For<IClock>().Use<SystemClock>();
                        x.For<IEnvironment>().Use<SystemEnvironment>();
                        x.For<IProcessHistory>().Use<RedisProcessHistory>();
                        x.For<IConfigPersistor>().Use<AppSettingsConfigPersistor>();
                        x.For<INetTpMessageBus>().Use<NetTpMessageBus>();
                        x.For<INetTpMessageBusImpl>().Use<EasyNetQBusImpl>();
                        x.For<IEasyNetQLogger>().Singleton().Use<EasyNetQNullLogger>();
                        x.For<INodeResponsabilityProvider>().Use<DefaultNodeResponsabilityProvider>();
                        x.For<INodeRemoteControl>().Use<NodeRemoteControl>();
                        x.For<INodeConfigPersistor>().Use<NodeConfigPersistor>();
                        x.For<IClientsPool<string>>().Singleton().Use( () => new ThreadwisePool() );

                        x.For<IRedisClient<string>>().Use( () =>
                            {
                                var pool = ObjectFactory.GetInstance<IClientsPool<string>>();
                                return pool.GetClient();
                            } );

                        if( ObjectFactory.TryGetInstance<ISbinAssemblyResolver>() == null )
                        {
                            x.For<ISbinAssemblyResolver>().Singleton().Use<SystemAssemblyResolver>();
                        }

                        x.For<INodeFinder>().Use<RedisNodeFinder>();
                    } );
        }
    }
}
