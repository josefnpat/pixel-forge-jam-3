using UnityEngine;

public class CheckboxUse : MonoBehaviour
{
    [SerializeField]
    private GameObject _mark;
    [SerializeField]
    private AudioClip _onActiveAudioClip;
    [SerializeField]
    private AudioClip _onNotActiveAudioClip;

    private SfxManager _sfxManager;

    public void Start()
    {
        _sfxManager = FindFirstObjectByType<SfxManager>();
    }

    public void Toggle()
    {
        bool target = !_mark.activeSelf;
        _mark.SetActive(target);
        if (target)
        {
            _sfxManager.Play(_onActiveAudioClip);
        }
        else
        {
            _sfxManager.Play(_onNotActiveAudioClip);
        }
    }

    public bool Value()
    {
        return _mark.activeSelf;
    }

}
