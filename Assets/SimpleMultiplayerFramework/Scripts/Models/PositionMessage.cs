
using System;
using MemoryPack;

[Serializable]
[MemoryPackable]
public partial class PositionMessage : INetworkMessage
{
    public MessageType messageType { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }

    public PositionMessage(float x, float y, float z)
    {
        messageType = MessageType.None;

        this.x = x;
        this.y = y;
        this.z = z;
    }
}