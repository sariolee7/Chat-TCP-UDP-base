using System;
using System.IO;

public class BinaryMessageProcessor : IMessageProcessor
{
    public byte[] Serialize(NetworkMessage message)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            writer.Write((int)message.Type);
            writer.Write(message.Data.Length);
            writer.Write(message.Data);

            return ms.ToArray();
        }
    }

    public NetworkMessage Deserialize(byte[] data)
    {
        using (MemoryStream ms = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            MessageType type = (MessageType)reader.ReadInt32();
            int length = reader.ReadInt32();
            byte[] messageData = reader.ReadBytes(length);

            return new NetworkMessage(type, messageData);
        }
    }
}