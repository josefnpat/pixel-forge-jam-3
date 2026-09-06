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
        _audioSource.clip = _grabAudioClip;
        _audioSource.Play();
    }

    public void PlayDrop()
    {
        _audioSource.clip = _dropAudioClip;
        _audioSource.Play();
    }
}
