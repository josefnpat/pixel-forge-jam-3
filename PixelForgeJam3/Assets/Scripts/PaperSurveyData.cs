using UnityEngine;

public class PaperSurveyData : MonoBehaviour
{
    [SerializeField]
    public CheckboxUse _checkboxUse;

    public bool Value()
    {
        return _checkboxUse.Value();
    }

}
