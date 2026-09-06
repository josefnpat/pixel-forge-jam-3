using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Draggable2D : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Vector3 _offset;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    private Sprite _originalSprite;
    [SerializeField]
    private Sprite _highlightSprite;
    public UnityEvent OnGrabStart = new UnityEvent();
    public UnityEvent OnGrabEnd = new UnityEvent();

    public void Start()
    {
        _originalSprite = _spriteRenderer.sprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _offset =  transform.position - GetMousePos();
            _spriteRenderer.sprite = _highlightSprite;
            OnGrabStart.Invoke();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            transform.position = GetMousePos() + _offset;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _spriteRenderer.sprite = _originalSprite;
        OnGrabEnd.Invoke();
    }

    private Vector3 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    public void SetSpriteRendererSortingOrder(int sortingOrder)
    {
        _spriteRenderer.sortingOrder = sortingOrder;
    }

    public int GetSpriteRendererSortingOrder()
    {
        return _spriteRenderer.sortingOrder;
    }

}
