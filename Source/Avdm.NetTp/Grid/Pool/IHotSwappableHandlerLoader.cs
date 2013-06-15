namespace Avdm.NetTp.Grid.Pool
{
    public interface IHotSwappableHandlerLoader
    {
        void Init( string arguments );
        void Register( IHotSwappableHandlerContainer container );
    }
}