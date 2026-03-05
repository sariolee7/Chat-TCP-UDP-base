using System.Text;
using TMPro;
using UnityEngine;

public class UdpClientUI: MonoBehaviour
{
    public int serverPort = 5555;
    public string serverAddress = "127.0.0.1";
    [SerializeField] private UDPClient clientReference;
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private TMP_Text messageText;


    private IClient _client;
    void Awake()
    {
        _client = clientReference;

        _client.Initialize(new BinaryMessageProcessor());
    }
    void Start()
    {
        _client.OnMessageReceived += HandleMessageReceived;
        _client.OnConnected += HandleConnection;
        _client.OnDisconnected += HandleDisconnection;
    }
    public void ConnectClient()
    {
        _client.ConnectToServer(serverAddress, serverPort);
    }
    public async void SendClientMessage()
    {
        if(!_client.isConnected)
        {
            Debug.Log("The client is not connected");
            return;
        }

        if(string.IsNullOrEmpty(messageInput.text))
        {
            Debug.Log("The chat entry is empty");
            return;
        }

        byte[] textData = Encoding.UTF8.GetBytes(messageInput.text);

        NetworkMessage message = new NetworkMessage(
            MessageType.Text,
            textData
        );


         await _client.SendMessageAsync(message);
        messageInput.text = "";
    }

    void HandleMessageReceived(NetworkMessage message)
    {
        Debug.Log($"[UI-Server] Received Type: {message.Type}");

        switch (message.Type)
        {
            case MessageType.Text:
                string text = Encoding.UTF8.GetString(message.Data);
                messageText.text = text;
                break;

            case MessageType.Image:
                Debug.Log("Image received (implement UI handling)");
                break;
        }
    }

    void HandleConnection()
    {
        Debug.Log("[UI-Client] Client Connected to Server");
    }
    void HandleDisconnection()
    {
        Debug.Log("[UI-Client] Client Disconnect from Server");
    }
}
