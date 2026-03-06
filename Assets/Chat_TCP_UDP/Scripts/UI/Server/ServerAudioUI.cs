using SFB;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ServerAudioUI : MonoBehaviour
{
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

    public bool HasAudio()
    {
        return _loadedAudioClip != null;
    }

    public byte[] GetAudioBytes()
    {
        if (_loadedAudioClip == null)
            return null;

        return WavUtility.FromAudioClip(_loadedAudioClip);
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

                Debug.Log("[UI-Server] Audio loaded");
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

    public void SetReceivedAudio(byte[] data)
    {
        _receivedAudioClip = WavUtility.ToAudioClip(data, 0, "receivedAudio");
        receivedAudioSource.clip = _receivedAudioClip;
    }
}