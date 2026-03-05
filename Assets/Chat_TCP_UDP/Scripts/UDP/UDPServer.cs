using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UDPServer : MonoBehaviour, IServer
{
    private UdpClient udpServer;
    private IPEndPoint remoteEndPoint;

    private IMessageProcessor _messageProcessor;

    public event Action<NetworkMessage> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public bool isServerRunning { get; private set; }

    public void Initialize(IMessageProcessor processor)
    {
        _messageProcessor = processor;
    }

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
                    // Socket cerrado intencionalmente
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

                    await udpServer.SendAsync(
                        connectedData,
                        connectedData.Length,
                        remoteEndPoint
                    );

                    continue;
                }

                NetworkMessage message =
                    _messageProcessor.Deserialize(result.Buffer);

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
            return;
        }

        byte[] data = _messageProcessor.Serialize(message);

        await udpServer.SendAsync(
            data,
            data.Length,
            remoteEndPoint
        );

        Debug.Log($"[UDP Server] Sent Type: {message.Type}");
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