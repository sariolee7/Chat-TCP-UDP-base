using SFB;
using System.Collections;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class UIServer : MonoBehaviour
{
    public int serverPort = 5555;

    [Header("Network Reference")]
    [SerializeField] private MonoBehaviour serverReference;

    private IServer _server;


    [Header("UI - Text")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private TMP_Text messageText;

    [Header("UI - Images")]
    [SerializeField] private RawImage receivedImage;
    [SerializeField] private RawImage sentImage;
    private Texture2D _loadedTexture;
    private Texture2D _receivedTexture;

    [Header("UI - Audio SEND")]
    [SerializeField] private Button playSentAudioButton;
    [SerializeField] private Slider sentAudioSlider;
    [SerializeField] private AudioSource sentAudioSource;

    [Header("UI - Audio RECEIVE")]
    [SerializeField] private Button playReceivedAudioButton;
    [SerializeField] private Slider receivedAudioSlider;
    [SerializeField] private AudioSource receivedAudioSource;

    private AudioClip _loadedAudioClip;
    private AudioClip _receivedAudioClip;

    void Awake()
    {
        _server = (IServer)serverReference;
        _server.Initialize(new BinaryMessageProcessor());
    }

    void Update()
    {
        UpdateSlider(sentAudioSource, sentAudioSlider);
        UpdateSlider(receivedAudioSource, receivedAudioSlider);
    }

    void UpdateSlider(AudioSource source, Slider slider)
    {
        if (source.clip == null)
            return;

        slider.value = source.time / source.clip.length;
    }

    void Start()
    {
        _server.OnMessageReceived += HandleMessageReceived;
        _server.OnConnected += () => Debug.Log("[UI-Server] Connected");
        _server.OnDisconnected += () => Debug.Log("[UI-Server]  Disconnected");
    }

    public void StartServer()
    {
        _server.StartServer(serverPort);
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

    public void LoadAudioFromExplorer()
    {
        var extensions = new[]
        {
        new ExtensionFilter("Audio Files", "wav", "mp3", "ogg")
    };

        string[] paths = StandaloneFileBrowser.OpenFilePanel(
            "Select Audio",
            "",
            extensions,
            false
        );

        if (paths.Length == 0)
            return;

        string path = paths[0];

        if (!File.Exists(path))
            return;

        StartCoroutine(LoadAudioCoroutine(path));
    }

    IEnumerator LoadAudioCoroutine(string path)
    {
        string url = "file://" + path;

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Audio load error: " + www.error);
            }
            else
            {
                _loadedAudioClip = DownloadHandlerAudioClip.GetContent(www);

                sentAudioSource.clip = _loadedAudioClip;

                Debug.Log($"[UI-Server] Audio loaded - Samples: {_loadedAudioClip.samples}, Channels: {_loadedAudioClip.channels}, Frequency: {_loadedAudioClip.frequency}, Length: {_loadedAudioClip.length}s");
            }
        }
    }

    public void PlaySentAudio()
    {
        if (_loadedAudioClip == null)
            return;

        sentAudioSource.Play();
    }

    public void PlayReceivedAudio()
    {
        if (_receivedAudioClip == null)
            return;

        receivedAudioSource.Play();
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

        if (HasText())
        {
            textSent = SendTextInternal();
        }

        if (HasImage())
        {
            imageSent = SendImageInternal();
        }

        bool audioSent = false;

        if (HasAudio())
        {
            audioSent = SendAudioInternal();
        }

        if (!textSent && !imageSent && !audioSent)
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

    private bool HasAudio()
    {
        return _loadedAudioClip != null;
    }
    private bool SendTextInternal()
    {
        byte[] textData = Encoding.UTF8.GetBytes(messageInput.text);

        NetworkMessage message = new NetworkMessage(
            MessageType.Text,
            textData
        );

        _server.SendMessageAsync(message);

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

        _server.SendMessageAsync(message);

        return true;
    }

    private bool SendAudioInternal()
    {
        if (_loadedAudioClip == null)
            return false;

        byte[] audioBytes = WavUtility.FromAudioClip(_loadedAudioClip);

        NetworkMessage message = new NetworkMessage(
            MessageType.Audio,
            audioBytes
        );

        _server.SendMessageAsync(message);

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

            case MessageType.Audio:
                _receivedAudioClip = WavUtility.ToAudioClip(message.Data, 0, "receivedAudio");
                receivedAudioSource.clip = _receivedAudioClip;
                break;
        }
    }
}