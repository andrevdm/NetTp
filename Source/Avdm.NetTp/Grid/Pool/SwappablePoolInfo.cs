using System;

namespace Avdm.NetTp.Grid.Pool
{
    public class SwappablePoolInfo
    {
        public SwappablePoolInfo( AppDomain domain, HotSwappableHandlers handlers )
        {
            Id = Guid.NewGuid();
            Domain = domain;
            Handlers = handlers;
            RunningHandlers = 0;
        }

        public Guid Id { get; private set; }
        public AppDomain Domain { get; set; }
        public HotSwappableHandlers Handlers { get; set; }
        public int RunningHandlers;
    }
}