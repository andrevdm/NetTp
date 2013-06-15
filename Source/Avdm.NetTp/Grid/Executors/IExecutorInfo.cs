using System;

namespace Avdm.NetTp.Grid.Executors
{
    public interface IExecutorInfo
    {
        Guid Id { get; }
        string Type { get; }
        object ChildId { get; }
        string ChildName { get; }
    }

    [Serializable]
    public class ExecutorInfo : IExecutorInfo
    {
        public Guid Id { get; private set; }
        public string Type { get; private set; }
        public object ChildId { get; private set; }
        public string ChildName { get; private set; }

        public ExecutorInfo( Guid id, string type, object childId, string childName )
        {
            Id = id;
            Type = type;
            ChildId = childId;
            ChildName = childName;
        }
    }
}