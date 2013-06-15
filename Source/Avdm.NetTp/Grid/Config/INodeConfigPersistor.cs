namespace Avdm.NetTp.Grid.Config
{
    public interface INodeConfigPersistor
    {
        NodeConfig LoadAppConfig( string applicationName );
        void SaveAppConfig( NodeConfig config );
    }
}