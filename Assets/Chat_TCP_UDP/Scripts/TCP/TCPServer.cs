using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class TCPServer : MonoBehaviour, IServer
{
    private TcpListener tcpListener;
    private TcpClient connectedClient;
    private NetworkStream networkStream;

    public bool isServerRunning { get; private set; }

    public event Action<NetworkMessage> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public async Task StartServer(int port)
    {
        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();

        Debug.Log("[Server] Server started, waiting for connections...");
        isServerRunning = true;

        connectedClient = await tcpListener.AcceptTcpClientAsync();

        Debug.Log("[Server] Client connected: " + connectedClient.Client.RemoteEndPoint);
        OnConnected?.Invoke();

        networkStream = connectedClient.GetStream();

        _ = ReceiveLoop();
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (connectedClient != null && connectedClient.Connected)
            {
                // Leer header
                byte[] header = new byte[8];
                await ReadExactAsync(header);

                using (MemoryStream headerStream = new MemoryStream(header))
                using (BinaryReader reader = new BinaryReader(headerStream))
                {
                    MessageType type = (MessageType)reader.ReadInt32();
                    int length = reader.ReadInt32();

                    // Leer payload
                    byte[] data = new byte[length];
                    await ReadExactAsync(data);

                    NetworkMessage message = new NetworkMessage(type, data);

                    OnMessageReceived?.Invoke(message);

                    Debug.Log($"[Server] Received {type} ({length} bytes)");
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
        if (networkStream == null || connectedClient == null || !connectedClient.Connected)
            return;

        byte[] data = SerializeMessage(message);

        await networkStream.WriteAsync(data, 0, data.Length);

        Debug.Log($"[Server] Sent {message.Type} ({data.Length} bytes)");
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
        if (!isServerRunning && connectedClient == null)
            return;

        networkStream?.Close();
        connectedClient?.Close();
        tcpListener?.Stop();

        networkStream = null;
        connectedClient = null;
        tcpListener = null;

        isServerRunning = false;

        Debug.Log("[Server] Disconnected");
        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}