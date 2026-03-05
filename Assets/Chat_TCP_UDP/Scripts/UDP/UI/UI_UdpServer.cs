using System.Text;
using TMPro;
using UnityEngine;

public class UdpServerUI : MonoBehaviour
{
    public int serverPort = 5555;
    [SerializeField] private UDPServer serverReference;
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private TMP_Text messageText;


    private IServer _server;
    void Awake()
    {
        _server = serverReference;


        _server.Initialize(new BinaryMessageProcessor());
    }
    void Start()
    {
        _server.OnMessageReceived += HandleMessageReceived;
        _server.OnConnected += HandleConnection;
        _server.OnDisconnected += HandleDisconnection;
    }
    public async void StartServer()
    {
        await _server.StartServer(serverPort);
    }
    public async void SendServerMessage()
    {
        if(!_server.isServerRunning){
            Debug.Log("The server is not running");
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

        await _server.SendMessageAsync(message);
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
        Debug.Log("[UI-Server] Client Connected to Server");
    }
    void HandleDisconnection()
    {
        Debug.Log("[UI-Server] Client Disconnect from Server");
    }
}
