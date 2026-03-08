using System;
using System.Threading.Tasks;

public interface IChatConnection
{
    event Action<NetworkMessage> OnMessageReceived;
    event Action OnConnected;
    event Action OnDisconnected;
    event Action<bool, string> OnMessageSent;
    
    Task SendMessageAsync(NetworkMessage message);
    void Disconnect();
}
