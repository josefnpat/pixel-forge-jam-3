using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Thing : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private int _maxInitCount = int.MaxValue;
    [SerializeField]
    private int _thresholdGoodMin = 0;
    [SerializeField]
    private int _thresholdGoodMax = int.MaxValue;
    [SerializeField]
    private int _thresholdBadMin = 0;
    [SerializeField]
    private int _thresholdBadMax = int.MaxValue;

    private SfxManager _sfxManager;
    private LineManager _lineManager;
    private BoxCollider2D _boxCollider2D;
    private Draggable2D _draggable2D;
    public UnityEvent<Thing> OnGrabStart = new UnityEvent<Thing>();
    public UnityEvent<Thing> OnGrabEnd = new UnityEvent<Thing>();
    public UnityEvent<Thing> OnUse = new UnityEvent<Thing>();
    public UnityEvent<int> OnSpriteRendererSortingOrder = new UnityEvent<int>();

    public UnityEvent<Thing> OnEventRemove = new UnityEvent<Thing>();
    public UnityEvent<GameObject> OnEventAddToPool = new UnityEvent<GameObject>();
    public UnityEvent<Thing, GameObject> OnEventCreateThing = new UnityEvent<Thing, GameObject>();
    public UnityEvent<Thing, GameObject> OnEventCreateCassete = new UnityEvent<Thing, GameObject>();
    public UnityEvent<Thing, InitThingsScriptableObject> OnEventCreateThings = new UnityEvent<Thing, InitThingsScriptableObject>();
    public UnityEvent<InitThingsScriptableObject> OnEventInit = new UnityEvent<InitThingsScriptableObject>();
    public UnityEvent OnEventGood = new UnityEvent();
    public UnityEvent OnEventBad = new UnityEvent();
    public UnityEvent<int> OnEventEarnMoney = new UnityEvent<int>();
    public UnityEvent OnEventAdvanceRandom = new UnityEvent();

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

    public bool WithinThreshold(int good, int bad)
    {
        return
            bad >= _thresholdBadMin &&
            bad <= _thresholdBadMax &&
            good >= _thresholdGoodMin &&
            good <= _thresholdGoodMax;
    }

    public bool CanInit(int initCount)
    {
        return initCount < _maxInitCount;
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
            // If you have an object reference it's own prefab, it accesses the actual game object,
            // which by default has "(Clone)" appended to it.
            string hackName = combineThingEvent.Prefab.name.Replace("(Clone)","");
            if (hackName == prefab.name)
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

    public void EventAddToPool(GameObject prefab)
    {
        OnEventAddToPool.Invoke(prefab);
    }

    public void EventGood()
    {
        OnEventGood.Invoke();
    }

    public void EventBad()
    {
        OnEventBad.Invoke();
    }

    public void EventSurveyProcess(bool markedIsGood)
    {
        PaperSurveyData paperSurveyData = GetComponent<PaperSurveyData>();
        if (paperSurveyData.Value())
        {
            if (markedIsGood)
            {
                OnEventGood.Invoke();
            }
            else
            {
                OnEventBad.Invoke();
            }
        }
        else
        {
            if (markedIsGood)
            {
                OnEventBad.Invoke();
            }
            else
            {
                OnEventGood.Invoke();
            }
        }
    }

    public void EventMoneyProcess(int value)
    {
        OnEventEarnMoney.Invoke(value);
    }

    public void EventPlaySfx(AudioClip clip)
    {
        _sfxManager.Play(clip);
    }

    public void EventAdvanceRandom()
    {
        OnEventAdvanceRandom.Invoke();
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

    public void EventCreateThing(GameObject prefab)
    {
        OnEventCreateThing.Invoke(this, prefab);
    }

    public void EventCreateCassete()
    {
        OnEventCreateCassete.Invoke(this, _lineManager.LinePrefab);
    }

    public void EventCreateThings(InitThingsScriptableObject initThingsScriptableObject)
    {
        OnEventCreateThings.Invoke(this, initThingsScriptableObject);
    }

    public void EventInit(InitThingsScriptableObject initThingsScriptableObject)
    {
        OnEventInit.Invoke(initThingsScriptableObject);
    }

    public void EventMusicPlay()
    {
        MusicData musicData = GetComponent<MusicData>();
        _lineManager.Play(musicData.Lines, this.Prefab);
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

    public void SetHighlight(bool value)
    {
        _draggable2D.SetHighlight(value);
    }
}
