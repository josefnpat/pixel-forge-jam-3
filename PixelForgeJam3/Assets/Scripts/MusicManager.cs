using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource;

    public void Play(AudioClip audioClip)
    {
        _audioSource.clip = audioClip;
        _audioSource.Play();
    }

    public void Stop()
    {
        _audioSource.Stop();
        _audioSource.clip = null;
    }
}
