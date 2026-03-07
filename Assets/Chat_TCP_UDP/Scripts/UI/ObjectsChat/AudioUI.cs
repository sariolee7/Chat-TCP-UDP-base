using System;
using SFB;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AudioUI : MonoBehaviour, IAudioUI
{
    [Header("UI - Audio SEND")]
    [SerializeField] private Slider sentAudioSlider;
    [SerializeField] private AudioSource sentAudioSource;

    [Header("UI - Audio RECEIVE")]
    [SerializeField] private Button playReceivedAudioButton;
    [SerializeField] private Slider receivedAudioSlider;
    [SerializeField] private AudioSource receivedAudioSource;

    public event Action OnAudioLoaded;
    public event Action OnAudioCleared;

    private AudioClip _loadedAudioClip;
    private AudioClip _receivedAudioClip;

    void Awake()
    {
        playReceivedAudioButton.interactable = false;
    }

    public bool HasAudio()
    {
        return _loadedAudioClip != null;
    }

    public AudioClip GetLoadedAudio()
    {
        return _loadedAudioClip;
    }

    public byte[] GetAudioBytes()
    {
        if (_loadedAudioClip == null)
            return null;

        return WavUtility.FromAudioClip(_loadedAudioClip);
    }


    public void SetWaitingForAudio()
    {
        playReceivedAudioButton.interactable = false;
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

                OnAudioLoaded?.Invoke();
            }
        }
    }

    public void ClearAudio()
    {
        _loadedAudioClip = null;
        sentAudioSource.clip = null;

        OnAudioCleared?.Invoke();
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

    public void SetReceivedAudio(byte[] data)
    {
        _receivedAudioClip = WavUtility.ToAudioClip(data, 0, "receivedAudio");
        receivedAudioSource.clip = _receivedAudioClip;

        playReceivedAudioButton.interactable = true;

    }

    public void UpdateAudioSliders()
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
}