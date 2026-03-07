using UnityEngine;
using UnityEngine.UI;

public class AudioMessageUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Slider slider;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        playButton.onClick.AddListener(PlayAudio);
    }

    public void Initialize(byte[] audioBytes)
    {
        AudioClip clip = WavUtility.ToAudioClip(audioBytes, 0, "chatAudio");
        audioSource.clip = clip;
    }

    void PlayAudio()
    {
        if (audioSource.clip == null)
            return;

        audioSource.Play();
    }

    void Update()
    {
        if (audioSource.clip == null)
            return;

        slider.value = audioSource.time / audioSource.clip.length;
    }
}