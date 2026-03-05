public class NetworkMessage
{
    public MessageType Type;
    public byte[] Data;

    public NetworkMessage(MessageType type, byte[] data)
    {
        Type = type;
        Data = data;
    }
}

public enum MessageType
{
    Text = 1,
    Image = 2,
    Audio = 3
}