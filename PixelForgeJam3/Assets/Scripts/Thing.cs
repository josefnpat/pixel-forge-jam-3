using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Thing : MonoBehaviour, IPointerDownHandler
{
    private SfxManager _sfxManager;
    private LineManager _lineManager;
    private BoxCollider2D _boxCollider2D;
    private Draggable2D _draggable2D;
    public UnityEvent<Thing> OnGrabStart = new UnityEvent<Thing>();
    public UnityEvent<Thing> OnGrabEnd = new UnityEvent<Thing>();
    public UnityEvent<Thing> OnUse = new UnityEvent<Thing>();
    public UnityEvent<int> OnSpriteRendererSortingOrder = new UnityEvent<int>();

    public UnityEvent<Thing> OnEventRemove = new UnityEvent<Thing>();
    public UnityEvent<Thing, GameObject> OnEventCreate = new UnityEvent<Thing, GameObject>();

    [SerializeField]
    private List<CombineThingEvent> _combineThingEvents;
    [SerializeField]
    private UnityEvent _useThingEvent;

    private Thing _lastThing;
    private Vector3 _tweenFrom;
    private Vector3 _tweenTo;
    private float _tweenDelta = 1;
    private float _tweenTime = 1;

    public GameObject Prefab { get; internal set; }
    public BoxCollider2D BoxCollider2D { get { return _boxCollider2D; } }

    public void Awake()
    {
        _sfxManager = FindFirstObjectByType<SfxManager>();
        _lineManager = FindFirstObjectByType<LineManager>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _draggable2D = GetComponent<Draggable2D>();
        if (_draggable2D)
        {
            _draggable2D.OnGrabStart.AddListener(GrabStart);
            _draggable2D.OnGrabEnd.AddListener(GrabEnd);
        }
    }

    public void Update()
    {
        if (_tweenDelta < _tweenTime)
        {
            _tweenDelta += Time.deltaTime;
            if (_tweenDelta > _tweenTime)
            {
                _tweenDelta = _tweenTime;
            }
            Vector3 movement = _tweenTo - _tweenFrom;
            transform.position = _tweenFrom + movement * ( _tweenDelta / _tweenTime);
        }
    }

    private void GrabStart()
    {
        _sfxManager.PlayGrab();
        OnGrabStart.Invoke(this);
    }

    private void GrabEnd()
    {
        _sfxManager.PlayDrop();
        OnGrabEnd.Invoke(this);
    }

    private void Use()
    {
        OnUse.Invoke(this);
    }

    public void SetSpriteRendererSortingOrder(int sortingOrder)
    {
        _draggable2D.SetSpriteRendererSortingOrder(sortingOrder);
        OnSpriteRendererSortingOrder.Invoke(sortingOrder);
    }

    public int GetSpriteRendererSortingOrder()
    {
        return _draggable2D.GetSpriteRendererSortingOrder();
    }

    public CombineThingEvent GetCombineThingEvent(GameObject prefab)
    {
        foreach (CombineThingEvent combineThingEvent in _combineThingEvents)
        {
            if (combineThingEvent.Prefab == prefab)
            {
                return combineThingEvent;
            }
        }
        return null;
    }

    public UnityEvent GetUseThingEvent()
    {
        return _useThingEvent;
    }

    public void EventRemove()
    {
        OnEventRemove.Invoke(this);
    }

    public void EventRemoveOther()
    {
        OnEventRemove.Invoke(_lastThing);
    }

    public void SetLastCombinedEvent(Thing thing)
    {
        _lastThing = thing;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Use();
        }
    }

    public void EventCreate(GameObject prefab)
    {
        OnEventCreate.Invoke(this, prefab);
    }

    public void EventMusicPlay()
    {
        LinesScriptableObjectScript lines = GetComponent<MusicData>().Lines;
        _lineManager.Play(lines);
    }

    public void EventMusicStop()
    {
        _lineManager.Stop();
    }

    public void TweenTo(Vector3 target)
    {
        _tweenFrom = transform.position;
        _tweenTo = target;
        _tweenDelta = 0f;
    }
}
