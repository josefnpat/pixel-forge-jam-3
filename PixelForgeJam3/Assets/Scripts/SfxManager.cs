using System;
using UnityEngine;

public class SfxManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip _grabAudioClip;
    [SerializeField]
    private AudioClip _dropAudioClip;

    public void PlayGrab()
    {
        Play(_grabAudioClip);
    }

    public void PlayDrop()
    {
        Play(_dropAudioClip);
    }

    public void Play(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.Play();
    }
}
