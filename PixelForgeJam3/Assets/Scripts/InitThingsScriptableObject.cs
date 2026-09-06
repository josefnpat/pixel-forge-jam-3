using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InitThingsScriptableObject", menuName = "Scriptable Objects/InitThingsScriptableObject")]
public class InitThingsScriptableObject : ScriptableObject
{
    public List<InitThing> InitThings;
}
