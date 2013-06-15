using System.Collections.Generic;

namespace Avdm.NetTp.Grid.Pool
{
    public interface IHotSwapHandlerStrategy
    {
        void Init( HotSwappableHandlerPool parent );
        void AddAssembliesToMonitor( IEnumerable<string> assemblyNames );
    }
}