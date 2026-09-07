using UnityEngine;

public class MusicData : MonoBehaviour
{
    [SerializeField]
    private LinesScriptableObjectScript _lines;
    public LinesScriptableObjectScript Lines { get { return _lines; } }
}
