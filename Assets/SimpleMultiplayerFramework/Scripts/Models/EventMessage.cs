using System;
using System.Collections.Generic;
using MemoryPack;

[Serializable]
[MemoryPackable]
public partial class EventMessage : INetworkMessage
{
    public MessageType messageType { get; set; }
    public string eventName { get; set; }
    public List<byte[]> eventArgs { get; set; }

    public EventMessage(string eventName, params object[] eventArgs)
    {
        messageType = MessageType.Event;
        this.eventName = eventName;

        if (eventArgs != null)
        {
            this.eventArgs = new List<byte[]>();

            foreach (var arg in eventArgs)
                this.eventArgs.Add(MemoryPackSerializer.Serialize(arg.GetType(), arg));
        }
    }
}
