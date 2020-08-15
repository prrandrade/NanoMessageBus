namespace NanoMessageBus.Abstractions.Enums
{
    public enum MessagePriority
    {
        NormalPriority,
        Level1Priority,
        Level2Priority,
        Level3Priority,
        Level4Priority
    }

    public enum SerializationEngine
    {
        NativeJson,
        DeflateJson,
        Protobuf,
        MessagePack
    }
}
