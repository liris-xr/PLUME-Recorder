namespace PLUME.Core.Object.SafeRef
{
    /// <summary>
    /// Holds the identifier message of a safe ref, built once and shared by every sample referencing the object
    /// instead of being rebuilt (with its GUID strings) for each sample. The message must never be mutated.
    /// </summary>
    internal interface IIdentifierPayloadCache<TPayload> where TPayload : class
    {
        TPayload Payload { get; set; }
    }
}
