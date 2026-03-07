using System.Text;
using UnityEditor.PackageManager;
using UnityEngine;

public class UIServer : MonoBehaviour
{
    public int serverPort = 5555;

    [Header("Network Reference")]
    [SerializeField] private MonoBehaviour serverReference;
    private IServer _server;

    [Header("Handlers")]
    [SerializeField] private TextUI textUI;
    [SerializeField] private ImageUI imageUI;
    [SerializeField] private AudioUI audioUI;

    void Awake()
    {
        _server = (IServer)serverReference;
    }

    void Start()
    {
        _server.OnMessageReceived += HandleMessageReceived;
        _server.OnConnected += () => Debug.Log("[UI-Server] Connected");
        _server.OnDisconnected += () => Debug.Log("[UI-Server] Disconnected");
    }

    public void StartServer()
    {
        _server.StartServer(serverPort);
    }

    public void LoadImageFromExplorer()
    {
        imageUI.LoadImageFromExplorer();
    }

    public void LoadAudioFromExplorer()
    {
        audioUI.LoadAudioFromExplorer();
    }


    public void Send()
    {
        if (!_server.isServerRunning)
        {
            Debug.Log("Server not running");
            return;
        }

        bool textSent = false;
        bool imageSent = false;
        bool audioSent = false;

        bool hasText = textUI.HasText();
        bool hasImage = imageUI.HasImage();
        bool hasAudio = audioUI.HasAudio();


        if (hasText)
            textSent = SendTextInternal();

         if (hasImage)
            imageSent = SendImageInternal();


        if (hasAudio)
            audioSent = SendAudioInternal();

        ChatInputStateController.Instance.OnMessageSent();

        if (!hasText && !imageSent && !hasAudio)
            Debug.Log("Nothing to send");

        if (imageSent)
            imageUI.ClearImage();

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

        _server.SendMessageAsync(message);

        textUI.InstantiateSentText(text);


        textUI.ClearInput();

        return true;
    }

    bool SendImageInternal()
    {
        Texture2D loadedTexture = imageUI.GetLoadedTexture();

        if (loadedTexture == null)
            return false;

        byte[] imageBytes = loadedTexture.EncodeToJPG(50);

        NetworkMessage message = new NetworkMessage(
            MessageType.Image,
            imageBytes
        );


        _server.SendMessageAsync(message);

        imageUI.InstantiateSentImage(loadedTexture);

        return true;
    }


    bool SendAudioInternal()
    {
        byte[] audioBytes = audioUI.GetAudioBytes();

        NetworkMessage message = new NetworkMessage(
            MessageType.Audio,
            audioBytes
        );

        _server.SendMessageAsync(message);

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

                imageUI.InstantiateReceivedImage(message.Data);

                break;

            case MessageType.Audio:

                audioUI.InstantiateReceivedAudio(message.Data);

                break;
        }
    }
}