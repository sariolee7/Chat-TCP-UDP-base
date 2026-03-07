using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class UDPClient : MonoBehaviour, IClient
{
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    public bool isConnected { get; private set; }

    public event Action<NetworkMessage> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    private UDPChunkSender chunkSender = new UDPChunkSender();
    private UDPChunkReceiver chunkReceiver = new UDPChunkReceiver();

    public async Task ConnectToServer(string ipAddress, int port)
    {
        udpClient = new UdpClient();
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

        isConnected = true;

        Debug.Log("[UDP Client] Connecting...");

        _ = ReceiveLoop();

        byte[] connectData = Encoding.UTF8.GetBytes("CONNECT");
        await udpClient.SendAsync(connectData, connectData.Length, remoteEndPoint);
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (isConnected && udpClient != null)
            {
                UdpReceiveResult result;

                try
                {
                    result = await udpClient.ReceiveAsync();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                if (!isConnected)
                    break;

                string rawMessage = Encoding.UTF8.GetString(result.Buffer);

                if (rawMessage == "CONNECTED")
                {
                    Debug.Log("[UDP Client] Server confirmed connection");
                    OnConnected?.Invoke();
                    continue;
                }

                byte[] fullMessage = chunkReceiver.HandlePacket(result.Buffer);

                if (fullMessage == null)
                    continue;

                NetworkMessage message = DeserializeMessage(fullMessage);

                Debug.Log($"[UDP Client] Received Type: {message.Type}");

                OnMessageReceived?.Invoke(message);
            }
        }
        catch (Exception ex)
        {
            if (isConnected)
                Debug.LogError("[UDP Client] Error: " + ex.Message);
        }
    }

    public async Task SendMessageAsync(NetworkMessage message)
    {
        if (!isConnected || remoteEndPoint == null || udpClient == null)
        {
            Debug.LogWarning("[UDP Client] Not connected.");
            return;
        }

        byte[] data = SerializeMessage(message);

        if (chunkSender.NeedsChunking(data))
        {
            var packets = chunkSender.CreateChunks(data);

            foreach (var packet in packets)
            {
                await udpClient.SendAsync(packet, packet.Length, remoteEndPoint);
            }
        }
        else
        {
            await udpClient.SendAsync(data, data.Length, remoteEndPoint);
        }

        Debug.Log($"[UDP Client] Sent Type: {message.Type}");
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
        if (!isConnected)
            return;

        isConnected = false;

        try
        {
            udpClient?.Close();
            udpClient?.Dispose();
        }
        catch { }

        udpClient = null;

        Debug.Log("[UDP Client] Disconnected");

        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}