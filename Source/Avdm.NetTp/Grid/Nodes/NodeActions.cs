using Avdm.NetTp.Grid.Executors;

namespace Avdm.NetTp.Grid.Nodes
{
    public static class NodeActions
    {
        public class Started
        {
        }

        public class ShutDown
        {
        }

        public class Initalising
        {
        }

        public class Supervising
        {
            public IExecutor Child { get; private set; }

            public Supervising( IExecutor child )
            {
                Child = child;
            }
        }

        public class ChildClosed
        {
            public IExecutor Child { get; private set; }

            public ChildClosed( IExecutor child )
            {
                Child = child;
            }
        }

        public class ChildFailed
        {
            public IExecutor Child { get; private set; }

            public ChildFailed( IExecutor child )
            {
                Child = child;
            }
        }

        public class ChildRestarted
        {
            public IExecutor Child { get; private set; }

            public ChildRestarted( IExecutor child )
            {
                Child = child;
            }
        }

        public class WorkerEnded
        {
        }
    }
}