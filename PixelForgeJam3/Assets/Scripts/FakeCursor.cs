using UnityEngine;
using UnityEngine.InputSystem;

public class FakeCursor : MonoBehaviour
{
    [SerializeField]
    private Vector2 _offset;
    [SerializeField]
    private Transform _cursor;

    public void Start()
    {
        OnApplicationFocus();
    }

    public void OnApplicationFocus()
    {
        Cursor.visible = false;
    }

    public void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        _cursor.position = mousePos + _offset;
    }
}
