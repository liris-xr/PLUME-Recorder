namespace PLUME.Core.Object
{
    public struct Identifier
    {
        public static Identifier Null { get; } = new(0, Guid.Null);
        
        public readonly int InstanceId;
        public readonly Guid Guid;

        public Identifier(int instanceId, Guid guid)
        {
            InstanceId = instanceId;
            Guid = guid;
        }
    }
}