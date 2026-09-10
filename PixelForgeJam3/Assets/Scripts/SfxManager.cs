using System.Collections.Generic;
using UnityEngine;

public class SfxManager : MonoBehaviour
{
    [SerializeField]
    private List<AudioSource> _audioSources;
    [SerializeField]
    private AudioClip _grabAudioClip;
    [SerializeField]
    private AudioClip _dropAudioClip;
    [SerializeField]
    private AudioClip _goodAudioClip;
    [SerializeField]
    private AudioClip _badAudioClip;
    [SerializeField]
    private AudioClip _initThingRandom;

    public void PlayGrab()
    {
        Play(_grabAudioClip);
    }

    public void PlayDrop()
    {
        Play(_dropAudioClip);
    }

    public void PlayGood()
    {
        Play(_goodAudioClip);
    }

    public void PlayBad()
    {
        Play(_badAudioClip);
    }

    public void PlayInitThingRandom()
    {
        Play(_initThingRandom);
    }

    private AudioSource FindFirstAudioSourceNotPlayingAudio()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }
        }
        return null;
    }

    public void Play(AudioClip clip)
    {
        AudioSource audioSource = FindFirstAudioSourceNotPlayingAudio();
        if (audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

}
