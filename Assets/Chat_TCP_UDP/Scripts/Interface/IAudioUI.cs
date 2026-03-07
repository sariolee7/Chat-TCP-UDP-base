using System;
using UnityEngine;

public interface IAudioUI
{
    event Action OnAudioLoaded;
    event Action OnAudioCleared;

    bool HasAudio();

    AudioClip GetLoadedAudio();
    byte[] GetAudioBytes();

    void LoadAudioFromExplorer();
    void ClearAudio();

    void PlaySentAudio();
    void PlayReceivedAudio();

    void SetReceivedAudio(byte[] data);

    void UpdateAudioSliders();
}