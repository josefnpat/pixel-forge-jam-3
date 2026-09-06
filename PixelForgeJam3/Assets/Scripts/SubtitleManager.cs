using TMPro;
using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _text;
    private float _delta;

    public void Start()
    {
        Stop();
    }

    public void Update()
    {
        _delta -= Time.deltaTime;
        if (_delta <= 0)
        {
            Stop();
        }
    }

    public void Play(string text, float time)
    {
        _text.text = text;
        _delta = time;
    }

    public void Stop()
    {
        if (_text.text != string.Empty)
        {
            _text.text = string.Empty;
        }
    }
}
