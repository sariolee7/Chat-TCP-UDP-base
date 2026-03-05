public interface IMessageProcessor
{
    byte[] Serialize(NetworkMessage message);
    NetworkMessage Deserialize(byte[] data);
}