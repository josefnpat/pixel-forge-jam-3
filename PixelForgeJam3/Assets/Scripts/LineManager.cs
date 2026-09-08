using System;
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
    private GameObject _linePrefab;
    private bool _loop;

    public GameObject LinePrefab { get { return _linePrefab; } }
    private Line _currentLine;
    private float _delta;

    public void Start()
    {
        _text.text = string.Empty;
    }

    public void Update()
    {
        if (_currentLine != null)
        {
            _delta += Time.deltaTime;
            if (_delta > _currentLine.AudioClip.length)
            {
                Stop();
                _currentLine = GetNextLine();
                if (_currentLine != null)
                {
                    Play(_currentLine);
                }
            }
        }
    }

    private Line GetNextLine()
    {
        if (_currentLine == null)
        {
            return _lines[0];
        }
        bool useNext = false;
        foreach (Line line in _lines)
        {
            if (useNext)
            {
                return line;
            }
            if (line == _currentLine)
            {
                useNext = true;
            }
        }
        if (_loop)
        {
            return _lines[0];
        }
        return null;
    }

    private void Play(Line line)
    {
        _currentLine = line;
        _delta = 0;
        _audioSource.clip = line.AudioClip;
        _audioSource.Play();
        _text.text = line.English;
    }

    public void Play(LinesScriptableObjectScript lines, GameObject prefab)
    {
        _lines = lines.Lines.ToList();
        Play(GetNextLine());
        _linePrefab = prefab;
        _loop = lines.Loop;
    }

    public void Stop()
    {
        _audioSource.Stop();
        _audioSource.clip = null;
        _text.text = string.Empty;
    }

}
