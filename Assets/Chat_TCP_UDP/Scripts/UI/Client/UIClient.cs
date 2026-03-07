using System.Text;
using UnityEngine;

public class UIClient : MonoBehaviour
{
    public int serverPort = 5555;
    public string serverAddress = "127.0.0.1";

    [Header("Network Reference")]
    [SerializeField] private MonoBehaviour clientReference;
    private IClient _client;

    [Header("Handlers")]
    [SerializeField] private TextUI textUI;
    [SerializeField] private ImageUI ImageUI;
    [SerializeField] private AudioUI audioUI;

    void Awake()
    {
        _client = (IClient)clientReference;
    }

    void Start()
    {
        _client.OnMessageReceived += HandleMessageReceived;
        _client.OnConnected += () => Debug.Log("[UI-Client] Connected");
        _client.OnDisconnected += () => Debug.Log("[UI-Client] Disconnected");
    }

    void Update()
    {
        audioUI.UpdateAudioSliders();
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

    public void PlaySentAudio()
    {
        audioUI.PlaySentAudio();
    }

    public void PlayReceivedAudio()
    {
        audioUI.PlayReceivedAudio();
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

        if (textUI.HasText())
            textSent = SendTextInternal();

        if (ImageUI.HasImage())
            imageSent = SendImageInternal();

        if (audioUI.HasAudio())
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
        byte[] textData = Encoding.UTF8.GetBytes(textUI.GetText());

        NetworkMessage message = new NetworkMessage(
            MessageType.Text,
            textData
        );

        _client.SendMessageAsync(message);

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

        return true;
    }

    void HandleMessageReceived(NetworkMessage message)
    {
        switch (message.Type)
        {
            case MessageType.Text:

                string text = Encoding.UTF8.GetString(message.Data);
                textUI.SetReceivedText(text);

                break;

            case MessageType.Image:

                ImageUI.SetReceivedImage(message.Data);

                break;

            case MessageType.Audio:

                audioUI.SetReceivedAudio(message.Data);

                break;
        }
    }
}