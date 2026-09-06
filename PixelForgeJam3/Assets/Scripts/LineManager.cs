using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LineManager : MonoBehaviour
{
[SerializeField]
    private TMP_Text _text;
    [SerializeField]
    private AudioSource _audioSource;
    private List<Line> _lines;
    private Line _currentLine;
    private float _delta;

    public void Start()
    {
        _text.text = string.Empty;
    }

    public void Update()
    {
        if (_currentLine == null)
        {
            if (_lines != null && _lines.Count > 0)
            {
                Line line = _lines[0];
                _lines.RemoveAt(0);
                Play(line);
            }
        }
        else
        {
            _delta += Time.deltaTime;
            if (_delta > _currentLine.AudioClip.length)
            {
                Stop();
            }
        }
    }

    private void Play(Line line)
    {
        _delta = 0;
        _audioSource.clip = line.AudioClip;
        _audioSource.Play();
        _text.text = line.English;
    }

    public void Play(LinesScriptableObjectScript lines)
    {
        _lines = lines.Lines.ToList();
    }

    public void Stop()
    {
        _audioSource.Stop();
        _audioSource.clip = null;
        _text.text = string.Empty;
        _currentLine = null;
    }


}
