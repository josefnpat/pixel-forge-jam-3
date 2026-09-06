using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LinesScriptableObjectScript", menuName = "Scriptable Objects/LinesScriptableObjectScript")]
public partial class LinesScriptableObjectScript : ScriptableObject
{
    public List<Line> Lines;
}
