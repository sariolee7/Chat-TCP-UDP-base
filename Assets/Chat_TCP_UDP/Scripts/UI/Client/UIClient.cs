using System.Text;
using UnityEngine;

public class UIClient : MonoBehaviour
{
    public int serverPort = 5555;
    public string serverAddress = "127.0.0.1";

    [Header("Network Reference")]
    [SerializeField] private MonoBehaviour clientReferenceTCP;
    [SerializeField] private MonoBehaviour clientReferenceUDP;
    private IClient _client;

    [Header("Handlers")]
    [SerializeField] private TextUI textUI;
    [SerializeField] private ImageUI ImageUI;
    [SerializeField] private AudioUI audioUI;

    void Awake()
    {
                if (ProtocolState.useTCP)
        {
            Debug.Log("Cliente usando TCP");
            _client = (IClient)clientReferenceTCP;
        }
        else
        {
            Debug.Log("Cliente usando UDP");
            _client = (IClient)clientReferenceUDP;
        }
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
        ImageUI.LoadImageFromExplorer();
    }

    public void LoadAudioFromExplorer()
    {
        audioUI.LoadAudioFromExplorer();
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
        bool audioSent = false;

        bool hasText = textUI.HasText();
        bool hasImage = ImageUI.HasImage();
        bool hasAudio = audioUI.HasAudio();


        if (hasText)
            textSent = SendTextInternal();

        if (hasImage)
            imageSent = SendImageInternal();
  

        if (hasAudio)
            audioSent = SendAudioInternal();


        if (!textSent && !imageSent && !audioSent)
            Debug.Log("Nothing to send");

        if (imageSent)
            ImageUI.ClearImage();

        if (audioSent)
            audioUI.ClearAudio();

        ChatInputStateController.Instance.OnMessageSent();

    }

    bool SendTextInternal()
    {
        string text = textUI.GetText();

        byte[] textData = Encoding.UTF8.GetBytes(textUI.GetText());

        NetworkMessage message = new NetworkMessage(
            MessageType.Text,
            textData
        );

        _client.SendMessageAsync(message);

        textUI.InstantiateSentText(text);

        textUI.ClearInput();

        return true;
    }

    bool SendImageInternal()
    {
        Texture2D loadedTexture = ImageUI.GetLoadedTexture();
        if (loadedTexture == null)
            return false;

        byte[] imageBytes = loadedTexture.EncodeToJPG(50);

        NetworkMessage message = new NetworkMessage(
            MessageType.Image,
            imageBytes
        );

        _client.SendMessageAsync(message);

        ImageUI.InstantiateSentImage(loadedTexture);

        return true;
    }


    bool SendAudioInternal()
    {
        byte[] audioBytes = audioUI.GetAudioBytes();

        NetworkMessage message = new NetworkMessage(
            MessageType.Audio,
            audioBytes
        );

        _client.SendMessageAsync(message);
        audioUI.InstantiateSentAudio(audioBytes);

        return true;
    }

    void HandleMessageReceived(NetworkMessage message)
    {
        switch (message.Type)
        {
            case MessageType.Text:

                string text = Encoding.UTF8.GetString(message.Data);
                textUI.InstantiateReceivedText(text);
                break;

            case MessageType.Image:

                ImageUI.InstantiateReceivedImage(message.Data);

                break;

            case MessageType.Audio:

                audioUI.InstantiateReceivedAudio(message.Data);

                break;
        }
    }
}