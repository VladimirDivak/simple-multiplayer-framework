using System;
using MemoryPack;

[Serializable]
public enum MessageType
{
    Event,
    None
}

[Serializable]
public enum NetworkEvent
{
    OnClientConnected,
    OnClientDisconnected,
    OnServerClosed
}

[MemoryPackable]
[MemoryPackUnion(0, typeof(EventMessage))]
[MemoryPackUnion(1, typeof(PositionMessage))]
public partial interface INetworkMessage
{
    public MessageType messageType { get; set; }
}
