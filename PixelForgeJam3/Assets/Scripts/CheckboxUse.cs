using UnityEngine;

public class CheckboxUse : MonoBehaviour
{
    [SerializeField]
    private GameObject _mark;

    public void Toggle()
    {
        _mark.SetActive(!_mark.activeSelf);
    }

    public bool Value()
    {
        return _mark.activeSelf;
    }

}
