using System;
using System.Threading.Tasks;

public interface IChatConnection
{
    event Action<NetworkMessage> OnMessageReceived;
    event Action OnConnected;
    event Action OnDisconnected;
    public Task SendMessageAsync(NetworkMessage message);
    public void Disconnect();
}
