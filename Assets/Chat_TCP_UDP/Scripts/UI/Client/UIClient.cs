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
    [SerializeField] private ImageUI imageUI;
    [SerializeField] private AudioUI audioUI;

    [Header("Connection Status")]
    [SerializeField] private MessageStatusUI connectionStatusUI;

    private enum LastSentType { None, Text, Image, Audio }
    private LastSentType _lastSentType = LastSentType.None;

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
        _client.OnConnected += HandleConnected;
        _client.OnDisconnected += HandleDisconnected;
        _client.OnMessageSent += HandleMessageSentResult;
    }

    void HandleConnected()
    {
        Debug.Log("[UI-Client] Connected");
        if (connectionStatusUI != null)
            connectionStatusUI.SetConnected();
    }

    void HandleDisconnected()
    {
        Debug.Log("[UI-Client] Disconnected");
        if (connectionStatusUI != null)
            connectionStatusUI.SetDisconnected();
    }

    void HandleMessageSentResult(bool success, string error)
    {
        switch (_lastSentType)
        {
            case LastSentType.Text:
                if (success) textUI.SetLastMessageSent();
                else textUI.SetLastMessageFailed(error);
                break;
            case LastSentType.Image:
                if (success) imageUI.SetLastMessageSent();
                else imageUI.SetLastMessageFailed(error);
                break;
            case LastSentType.Audio:
                if (success) audioUI.SetLastMessageSent();
                else audioUI.SetLastMessageFailed(error);
                break;
        }
        _lastSentType = LastSentType.None;
    }

    public void ConnectClient()
    {
        _client.ConnectToServer(serverAddress, serverPort);
    }

    public void DisconnectClient()
    {
        if (_client != null && _client.isConnected)
        {
            _client.Disconnect();
        }
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
        if (!_client.isConnected)
        {
            Debug.Log("Client not connected");
            return;
        }

        bool hasText = textUI.HasText();
        bool hasImage = imageUI.HasImage();
        bool hasAudio = audioUI.HasAudio();

        if (!hasText && !hasImage && !hasAudio)
        {
            Debug.Log("Nothing to send");
            return;
        }

        if (hasText)
            SendText();

        if (hasImage)
            SendImage();

        if (hasAudio)
            SendAudio();

        if (ChatInputStateController.Instance != null)
            ChatInputStateController.Instance.OnMessageSent();
    }

    void SendText()
    {
        string text = textUI.GetText();
        byte[] textData = Encoding.UTF8.GetBytes(text);
        NetworkMessage message = new NetworkMessage(MessageType.Text, textData);

        textUI.InstantiateSentText(text);
        _lastSentType = LastSentType.Text;

        _client.SendMessageAsync(message);
        textUI.ClearInput();
    }

    void SendImage()
    {
        Texture2D loadedTexture = imageUI.GetLoadedTexture();
        if (loadedTexture == null)
            return;

        byte[] imageBytes = loadedTexture.EncodeToJPG(50);
        NetworkMessage message = new NetworkMessage(MessageType.Image, imageBytes);

        imageUI.InstantiateSentImage(loadedTexture);
        _lastSentType = LastSentType.Image;

        _client.SendMessageAsync(message);
        imageUI.ClearImage();
    }

    void SendAudio()
    {
        byte[] audioBytes = audioUI.GetAudioBytes();
        if (audioBytes == null)
            return;

        NetworkMessage message = new NetworkMessage(MessageType.Audio, audioBytes);

        audioUI.InstantiateSentAudio(audioBytes);
        _lastSentType = LastSentType.Audio;

        _client.SendMessageAsync(message);
        audioUI.ClearAudio();
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