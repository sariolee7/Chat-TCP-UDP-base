using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UDPClient : MonoBehaviour, IClient
{
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    private IMessageProcessor _messageProcessor;

    public bool isConnected { get; private set; }

    public event Action<NetworkMessage> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public void Initialize(IMessageProcessor processor)
    {
        _messageProcessor = processor;
    }

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
                    // El socket fue cerrado intencionalmente
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

                NetworkMessage message =
                    _messageProcessor.Deserialize(result.Buffer);

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

        byte[] data = _messageProcessor.Serialize(message);

        await udpClient.SendAsync(data, data.Length, remoteEndPoint);

        Debug.Log($"[UDP Client] Sent Type: {message.Type}");
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