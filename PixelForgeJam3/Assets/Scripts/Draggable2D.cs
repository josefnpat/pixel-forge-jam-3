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
            Vector3 target = GetMousePos() + _offset;

            Vector3 spriteHalfSize = _spriteRenderer.bounds.extents;
            float depth = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
            Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, depth));
            Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, depth));
            float minX = bottomLeft.x + spriteHalfSize.x;
            float maxX = topRight.x - spriteHalfSize.x;
            float minY = bottomLeft.y + spriteHalfSize.y;
            float maxY = topRight.y - spriteHalfSize.y;
            target.x = Mathf.Clamp(target.x, minX, maxX);
            target.y = Mathf.Clamp(target.y, minY, maxY);

            transform.position = target;
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

    public void SetHighlight(bool value)
    {
        _spriteRenderer.sprite = value ? _highlightSprite : _originalSprite;
    }
}
