using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using SFB;
using System.IO;

public class UIClient : MonoBehaviour
{
    public int serverPort = 5555;
    public string serverAddress = "127.0.0.1";

    [Header("Network Reference")]
    [SerializeField] private MonoBehaviour clientReference;

    private IClient _client;

    [Header("UI - Text")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private TMP_Text messageText;

    [Header("UI - Images")]
    [SerializeField] private RawImage receivedImage;
    [SerializeField] private RawImage sentImage;
    private Texture2D _loadedTexture;
    private Texture2D _receivedTexture;


    void Awake()
    {
        _client = (IClient)clientReference;
        _client.Initialize(new BinaryMessageProcessor());
    }
    void Start()
    {
        _client.OnMessageReceived += HandleMessageReceived;
        _client.OnConnected += () => Debug.Log("[UI-Client] Connected");
        _client.OnDisconnected += () => Debug.Log("[UI-Client] Disconnected");
    }

    public void ConnectClient()
    {
        _client.ConnectToServer(serverAddress, serverPort);
    }

    public void LoadImageFromExplorer()
    {
        var extensions = new[]
        {
        new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
    };

        string[] paths = StandaloneFileBrowser.OpenFilePanel(
            "Select Image",
            "",
            extensions,
            false
        );

        if (paths.Length == 0)
            return;

        string path = paths[0];

        if (!File.Exists(path))
            return;

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(File.ReadAllBytes(path));

        _loadedTexture = tex;
        sentImage.texture = _loadedTexture;

        Debug.Log("[UI-Server] Image loaded and stored as Texture2D");
    }


    public void Send()
    {
        if (!_client.isConnected)
        {
            Debug.Log("Client not connected");
            return;
        }

        bool textSent = false;
        bool imageSent = false;

        if (HasText())
        {
            textSent = SendTextInternal();
        }

        if (HasImage())
        {
            imageSent = SendImageInternal();
        }

        if (!textSent && !imageSent)
        {
            Debug.Log("Nothing to send");
        }
    }

    private bool HasText()
    {
        return !string.IsNullOrEmpty(messageInput.text);
    }

    private bool HasImage()
    {
        return _loadedTexture != null;
    }


    private bool SendTextInternal()
    {
        byte[] textData = Encoding.UTF8.GetBytes(messageInput.text);

        NetworkMessage message = new NetworkMessage(
            MessageType.Text,
            textData
        );

        _client.SendMessageAsync(message);

        messageInput.text = "";
        return true;
    }

    private bool SendImageInternal()
    {
        if (_loadedTexture == null)
            return false;

        byte[] imageBytes = _loadedTexture.EncodeToJPG(50);

        NetworkMessage message = new NetworkMessage(
            MessageType.Image,
            imageBytes
        );

        _client.SendMessageAsync(message);

        return true;
    }


    void HandleMessageReceived(NetworkMessage message)
    {
        switch (message.Type)
        {
            case MessageType.Text:
                string text = Encoding.UTF8.GetString(message.Data);
                messageText.text = text;
                break;

            case MessageType.Image:

                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(message.Data);

                _receivedTexture = tex;
                receivedImage.texture = _receivedTexture;
                break;
        }
    }
}