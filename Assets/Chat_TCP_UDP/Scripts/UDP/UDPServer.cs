using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class UDPServer : MonoBehaviour, IServer
{
    private UdpClient udpServer;
    private IPEndPoint remoteEndPoint;

    public int MaxMessageSize = 2 * 1024 * 1024;

    public event Action<NetworkMessage> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<bool, string> OnMessageSent;

    public bool isServerRunning { get; private set; }

    private UDPChunkSender chunkSender = new UDPChunkSender();
    private UDPChunkReceiver chunkReceiver = new UDPChunkReceiver();

    public Task StartServer(int port)
    {
        udpServer = new UdpClient(port);
        isServerRunning = true;

        Debug.Log("[UDP Server] Server started. Waiting for messages...");

        _ = ReceiveLoop();

        return Task.CompletedTask;
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (isServerRunning && udpServer != null)
            {
                UdpReceiveResult result;

                try
                {
                    result = await udpServer.ReceiveAsync();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                if (!isServerRunning)
                    break;

                string rawMessage = Encoding.UTF8.GetString(result.Buffer);

                if (rawMessage == "CONNECT")
                {
                    remoteEndPoint = result.RemoteEndPoint;

                    Debug.Log("[UDP Server] Client connected: " + remoteEndPoint);

                    OnConnected?.Invoke();

                    byte[] connectedData = Encoding.UTF8.GetBytes("CONNECTED");

                    await udpServer.SendAsync(connectedData, connectedData.Length, remoteEndPoint);

                    continue;
                }

                byte[] fullMessage = chunkReceiver.HandlePacket(result.Buffer);

                if (fullMessage == null)
                    continue;

                NetworkMessage message = DeserializeMessage(fullMessage);

                Debug.Log($"[UDP Server] Received Type: {message.Type}");

                OnMessageReceived?.Invoke(message);
            }
        }
        catch (Exception ex)
        {
            if (isServerRunning)
                Debug.LogError("[UDP Server] Error: " + ex.Message);
        }
    }

    public async Task SendMessageAsync(NetworkMessage message)
    {
        if (!isServerRunning || remoteEndPoint == null || udpServer == null)
        {
            Debug.LogWarning("[UDP Server] No client connected.");
            OnMessageSent?.Invoke(false, "No client connected");
            return;
        }

        byte[] data = SerializeMessage(message);

        if (data.Length > MaxMessageSize)
        {
            Debug.LogWarning($"[UDP Server] Message too large: {data.Length} bytes (max: {MaxMessageSize})");
            OnMessageSent?.Invoke(false, $"Message too large: {data.Length} bytes");
            return;
        }

        try
        {
            if (chunkSender.NeedsChunking(data))
            {
                var packets = chunkSender.CreateChunks(data);

                foreach (var packet in packets)
                {
                    await udpServer.SendAsync(packet, packet.Length, remoteEndPoint);
                }
            }
            else
            {
                await udpServer.SendAsync(data, data.Length, remoteEndPoint);
            }

            Debug.Log($"[UDP Server] Sent Type: {message.Type}");
            OnMessageSent?.Invoke(true, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[UDP Server] Send failed: {ex.Message}");
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

    private NetworkMessage DeserializeMessage(byte[] data)
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

    public void Disconnect()
    {
        if (!isServerRunning)
            return;

        isServerRunning = false;

        try
        {
            udpServer?.Close();
            udpServer?.Dispose();
        }
        catch { }

        udpServer = null;

        Debug.Log("[UDP Server] Disconnected");

        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}