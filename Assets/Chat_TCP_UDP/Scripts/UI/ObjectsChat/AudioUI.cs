using System;
using SFB;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AudioUI : MonoBehaviour, IAudioUI
{
    [Header("UI - Audio SEND (Preview)")]
    [SerializeField] private AudioSource sentAudioSource;
    [SerializeField] private Slider sentAudioSlider;
    [SerializeField] private Button playSentButton;

    [Header("Chat Content")]
    [SerializeField] private Transform chatContent;

    [Header("Prefabs")]
    [SerializeField] private GameObject sentAudioPrefab;
    [SerializeField] private GameObject receivedAudioPrefab;

    [Header("Colors")]
    [SerializeField] private Color sentColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color receivedColor = new Color(0.8f, 0.8f, 0.8f);

    public event Action OnAudioLoaded;
    public event Action OnAudioCleared;

    private AudioClip _loadedAudioClip;

    void Awake()
    {
        if (sentAudioSource == null)
            sentAudioSource = GetComponent<AudioSource>();

        if (playSentButton != null)
            playSentButton.onClick.AddListener(PlaySentAudio);

        sentAudioSource.clip = null;
    }

    void Update()
    {
        UpdateAudioSlider();
    }

    public bool HasAudio() => _loadedAudioClip != null;
    public AudioClip GetLoadedAudio() => _loadedAudioClip;

    public byte[] GetAudioBytes()
    {
        if (_loadedAudioClip == null) return null;
        return WavUtility.FromAudioClip(_loadedAudioClip);
    }

    public void LoadAudioFromExplorer()
    {
        var extensions = new[] { new ExtensionFilter("Audio Files", "wav", "mp3", "ogg") };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Audio", "", extensions, false);

        if (paths.Length == 0 || !File.Exists(paths[0])) return;

        StartCoroutine(LoadAudioCoroutine(paths[0]));
    }

    private IEnumerator LoadAudioCoroutine(string path)
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
                OnAudioLoaded?.Invoke();
            }
        }
    }

    public void ClearAudio()
    {
        _loadedAudioClip = null;
        sentAudioSource.clip = null;
        
        if (sentAudioSlider != null)
            sentAudioSlider.value = 0;
            
        OnAudioCleared?.Invoke();
    }

    public void PlaySentAudio()
    {
        if (_loadedAudioClip == null) return;
        sentAudioSource.Play();
    }

    public void UpdateAudioSlider()
    {
        if (sentAudioSlider == null || sentAudioSource == null) return;
        if (sentAudioSource.clip == null) return;
        
        sentAudioSlider.value = sentAudioSource.time / sentAudioSource.clip.length;
    }


    public void InstantiateSentAudio(byte[] audioBytes)
    {
        if (sentAudioPrefab == null || chatContent == null) return;

        GameObject msg = Instantiate(sentAudioPrefab, chatContent);
        AudioMessageUI audioMsg = msg.GetComponent<AudioMessageUI>();

        if (audioMsg != null)
        {
            audioMsg.Initialize(audioBytes);
        }

        Image bg = msg.GetComponent<Image>();
        if (bg != null)
            bg.color = sentColor;
    }

    public void InstantiateReceivedAudio(byte[] audioBytes)
    {
        if (receivedAudioPrefab == null || chatContent == null) return;

        GameObject msg = Instantiate(receivedAudioPrefab, chatContent);
        AudioMessageUI audioMsg = msg.GetComponent<AudioMessageUI>();

        if (audioMsg != null)
        {
            audioMsg.Initialize(audioBytes);
        }

        Image bg = msg.GetComponent<Image>();
        if (bg != null)
            bg.color = receivedColor;
    }
}