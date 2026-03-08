using System;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class TCPClient : MonoBehaviour, IClient
{
    private TcpClient tcpClient;
    private NetworkStream networkStream;

    public bool isConnected { get; private set; }

    public event Action<NetworkMessage> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<bool, string> OnMessageSent;

    public async Task ConnectToServer(string ip, int port)
    {
        tcpClient = new TcpClient();

        await tcpClient.ConnectAsync(ip, port);
        networkStream = tcpClient.GetStream();

        isConnected = true;
        Debug.Log("[Client] Connected to server");
        OnConnected?.Invoke();

        _ = ReceiveLoop();
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (tcpClient != null && tcpClient.Connected)
            {
                byte[] header = new byte[8];
                await ReadExactAsync(header);

                using (MemoryStream headerStream = new MemoryStream(header))
                using (BinaryReader reader = new BinaryReader(headerStream))
                {
                    MessageType type = (MessageType)reader.ReadInt32();
                    int length = reader.ReadInt32();

                    byte[] data = new byte[length];
                    await ReadExactAsync(data);

                    NetworkMessage message = new NetworkMessage(type, data);

                    OnMessageReceived?.Invoke(message);

                    Debug.Log($"[Client] Received {type} ({length} bytes)");
                }
            }
        }
        finally
        {
            Disconnect();
        }
    }

    private async Task ReadExactAsync(byte[] buffer)
    {
        int totalRead = 0;

        while (totalRead < buffer.Length)
        {
            int read = await networkStream.ReadAsync(
                buffer,
                totalRead,
                buffer.Length - totalRead
            );

            if (read == 0)
                return;

            totalRead += read;
        }
    }

    public async Task SendMessageAsync(NetworkMessage message)
    {
        if (!isConnected || networkStream == null)
        {
            Debug.Log("[Client] Not connected to server");
            OnMessageSent?.Invoke(false, "Not connected to server");
            return;
        }

        byte[] data = SerializeMessage(message);

        try
        {
            await networkStream.WriteAsync(data, 0, data.Length);

            Debug.Log($"[Client] Sent {message.Type} ({data.Length} bytes)");
            OnMessageSent?.Invoke(true, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Client] Send failed: {ex.Message}");
            OnMessageSent?.Invoke(false, ex.Message);
        }
    }

    private byte[] SerializeMessage(NetworkMessage message)
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

    public void Disconnect()
    {
        if (!isConnected)
            return;

        isConnected = false;

        networkStream?.Close();
        tcpClient?.Close();

        networkStream = null;
        tcpClient = null;

        OnDisconnected?.Invoke();
        Debug.Log("[Client] Disconnected");
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}