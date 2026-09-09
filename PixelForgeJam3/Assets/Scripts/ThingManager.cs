using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThingManager : MonoBehaviour
{
    public int debugAddCount = 10;
    public bool debugAddGood = false;
    public bool debugAddBad = false;

    private int _eventGood = 0;
    private int _eventBad = 0;
    private int _currentSortingOrder = 0;

    private int _eventGoodGameOver = 50;
    private int _eventBadGameOver = 50;
    private int _eventGameOverThreshold = 60;

    private Dictionary<GameObject, int> _thingInitCounts = new Dictionary<GameObject, int>();

    [SerializeField]
    private List<GameObject> _endBoxPrefabs;
    [SerializeField]
    private List<GameObject> _poolPrefabs;

    [SerializeField]
    private InitThingsScriptableObject _initThings;
    [SerializeField]
    private InitThingsScriptableObject _gameOverBothThings;
    [SerializeField]
    private InitThingsScriptableObject _gameOverGoodThings;
    [SerializeField]
    private InitThingsScriptableObject _gameOverBadThings;
    

    [SerializeField]
    private InitThingsScriptableObject _initThingsOnInactive;
    private float _inactiveDelta = 0;
    private float _inactiveTime = 10;
    private bool _inactiveTriggered = false;

    [SerializeField]
    private GameObject _moneyPrefab;

    private List<Thing> _spawnedThings = new List<Thing>();
    private List<Thing> _randomThings = new List<Thing>();
    private Thing _grabbedThing;
    private Thing _grabbedIsHighlightingThing;

    private float _randomThingSpawnDelta = 1;
    private float _randomThingSpawnMax = 8;
    private float _randomThingSpawnTimeMin = 5;
    private float _randomThingSpawnTimeMax = 10;
    private float _offscreenPositionY = 2f;
    private float _randomPositionXMin = -0.25f;
    private float _randomPositionXMax = 0.25f;
    private float _randomPositionYMin = -0.25f;
    private float _randomPositionYMax = 0.25f;

    private double _gameTime;
    private Thing _lastThingInit;
    private bool _gameOver = false;

    public double GameTime { get { return _gameTime; } }

    public void Start()
    {
        System.DateTimeOffset utcNow = System.DateTimeOffset.UtcNow;
        long utcUnixSeconds = utcNow.ToUnixTimeSeconds();
        long offsetSeconds = (long)System.TimeZoneInfo.Local.GetUtcOffset(utcNow).TotalSeconds;
        _gameTime = utcUnixSeconds + offsetSeconds;
        InitThings(_initThings);
    }

    public void Update()
    {
        if (debugAddGood)
        {
            debugAddGood = false;
            EventGood(debugAddCount);
        }
        if (debugAddBad)
        {
            debugAddBad = false;
            EventBad(debugAddCount);
        }
        _gameTime += Time.deltaTime;
        if (!_gameOver)
        {
            _randomThingSpawnDelta -= Time.deltaTime;
        }
        if (_randomThingSpawnDelta <= 0)
        {
            if (_randomThings.Count < _randomThingSpawnMax)
            {
                _randomThingSpawnDelta = Random.Range(_randomThingSpawnTimeMin, _randomThingSpawnTimeMax);
                InitThingRandom();
            }
        }
        if (!_inactiveTriggered)
        {
            _inactiveDelta += Time.deltaTime;
            if (_inactiveDelta > _inactiveTime)
            {
                _inactiveTriggered = true;
                foreach (InitThing initThing in _initThingsOnInactive.InitThings)
                {
                    InitThing(initThing);
                }
            }
        }

        if (_grabbedIsHighlightingThing != null)
        {
            _grabbedIsHighlightingThing.SetHighlight(false);
        }
        if (_grabbedThing != null)
        {
            List<Thing> intersectingThings = FindIntersectingThings(_grabbedThing);
            Thing closestIntersectingThing = GetClosestThing(intersectingThings);
            if (closestIntersectingThing && _grabbedThing.GetCombineThingEvent(closestIntersectingThing.Prefab) != null)
            {
                _grabbedIsHighlightingThing = closestIntersectingThing;
                _grabbedIsHighlightingThing.SetHighlight(true);
            }
        }

        if (!_gameOver && (_eventGood > _eventGameOverThreshold || _eventBad > _eventGameOverThreshold))
        {
            _gameOver = true;
            bool goodEnd = _eventGood > _eventGoodGameOver;
            bool badEnd = _eventBad > _eventBadGameOver;
            if (goodEnd && badEnd)
            {
                InitThings(_gameOverBothThings);
            }
            else if (goodEnd)
            {
                InitThings(_gameOverGoodThings);
            }
            else if (badEnd)
            {
                InitThings(_gameOverBadThings);
            }
            else
            {
                Debug.LogWarning("You ended the game, but had no end conditions.");
            }
        }

    }

    private void InitThings(InitThingsScriptableObject things)
    {
        foreach (InitThing initThing in things.InitThings)
        {
            InitThing(initThing);
        }
    }

    private void InitThingRandom()
    {
        List<GameObject> validPoolPrefabs = ValidPoolPrefabs();
        if (validPoolPrefabs.Count > 0)
        {
            Thing thing = InitThingFromOffscreen(validPoolPrefabs[Random.Range(0,validPoolPrefabs.Count)]);
            if (thing)
            {
                _randomThings.Add(thing);
            }
        }
    }

    private Thing InitThingFromOffscreen(GameObject prefab)
    {
        Thing thing = InitThing(prefab);
        if (thing)
        {
            float x = Random.Range(_randomPositionXMin,_randomPositionXMax);
            float y = Random.Range(_randomPositionYMin,_randomPositionYMax);
            TweenThingToPosition(thing, new Vector3(x,y,0));
            SetRandomRotationFull(thing);
            return thing;
        }
        return null;
    }

    private List<GameObject> ValidPoolPrefabs()
    {
        List<GameObject> validPoolPrefabs = new List<GameObject>();
        if (_lastThingInit && validPoolPrefabs.Count > 1)
        {
            if (validPoolPrefabs.Contains(_lastThingInit.Prefab))
            {
                validPoolPrefabs.Remove(_lastThingInit.Prefab);
            }
        }
        foreach (GameObject prefab in _poolPrefabs)
        {
            Thing thing = prefab.GetComponent<Thing>();
            if (thing.WithinThreshold(_eventGood, _eventBad) && thing.CanInit(GetThingInitCount(prefab)))
            {
                validPoolPrefabs.Add(prefab);
            }
        }
        Debug.Log($"There are {_poolPrefabs.Count} initThing in the pool and {validPoolPrefabs.Count} of them are valid.");
        return validPoolPrefabs;
    }

    private void SetRandomRotationFull(Thing thing)
    {
        thing.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
    }

    private void SetRandomRotationPartial(Thing thing)
    {
        thing.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-20f, 20f));
    }

    private void TweenThingToPosition(Thing thing, Vector3 position)
    {
        float x = position.x;
        float y = position.y;
        thing.transform.position = new Vector3(x, _offscreenPositionY, 0);
        thing.TweenTo(new Vector3(x, y, 0));
    }

    private Thing InitThing(InitThing initThing)
    {
        Debug.Log($"InitThing:{initThing.Prefab}");
        Thing newThing = InitThing(initThing.Prefab);
        if (newThing)
        {
            SetRandomRotationPartial(newThing);
            TweenThingToPosition(newThing, initThing.Position);
            return newThing;
        }
        return null;
    }

    private Thing CreateThing(GameObject prefab)
    {
        return InitThing(prefab, true);
    }

    private int GetThingInitCount(GameObject prefab)
    {
        if (!_thingInitCounts.ContainsKey(prefab))
        {
            _thingInitCounts[prefab] = 0;
        }
        return _thingInitCounts[prefab];
    }

    private void IncrementThingInitCount(GameObject prefab)
    {
        if (!_thingInitCounts.ContainsKey(prefab))
        {
            _thingInitCounts[prefab] = 0;
        }
        _thingInitCounts[prefab]++;
    }

    private Thing InitThing(GameObject prefab, bool ignoreInitCount = false)
    {
        int count = GetThingInitCount(prefab);
        if (ignoreInitCount || prefab.GetComponent<Thing>().CanInit(count))
        {
            IncrementThingInitCount(prefab);

            GameObject go = Instantiate(prefab);
            Thing thing = go.GetComponent<Thing>();
            thing.Prefab = prefab;

            if (!thing.IsEndBox)
            {
                foreach (GameObject endBoxPrefab in _endBoxPrefabs)
                {
                    CombineThingEvent combineThingEvent = new CombineThingEvent();
                    combineThingEvent.Prefab = endBoxPrefab;
                    combineThingEvent.UnityEvent = new UnityEngine.Events.UnityEvent();
                    combineThingEvent.UnityEvent.AddListener(thing.EventRemove);
                    thing.AddCombineThingEvent(combineThingEvent);
                }
            }

            thing.OnEventAddToPool.AddListener(EventAddToPool);
            thing.OnEventCreateThing.AddListener(EventCreateThing);
            thing.OnEventCreateCassete.AddListener(EventCreateCassete);
            thing.OnEventCreateThings.AddListener(EventCreateThings);
            thing.OnEventInit.AddListener(EventInit);
            thing.OnEventRemove.AddListener(EventRemove);
            thing.OnEventGood.AddListener(EventGood);
            thing.OnEventBad.AddListener(EventBad);
            thing.OnEventEarnMoney.AddListener(EventEarnMoney);
            thing.OnEventAdvanceRandom.AddListener(EventAdvanceRandom);
            thing.OnGrabStart.AddListener(GrabStart);
            thing.OnGrabEnd.AddListener(GrabEnd);
            thing.OnUse.AddListener(Use);
            
            _spawnedThings.Add(thing);
            BumpSortOrder(thing);
            
            _lastThingInit = thing;

            return thing;
        }

        return null;
    }

    private void EventCreateCassete(Thing thing, GameObject prefab)
    {
        EventCreateThing(thing, prefab);
    }

    private void EventEarnMoney(int value)
    {
        for (int i = 0; i < value; i++)
        {
            InitThingFromOffscreen(_moneyPrefab);
        }
    }

    private void EventAdvanceRandom()
    {
        _randomThingSpawnDelta = 0.25f;
    }

    private void EventGood()
    {
        EventGood(1);
    }

    private void EventGood(int count)
    {
        _eventGood+=count;
        Debug.Log($"Good++: {_eventGood}");
    }

    private void EventBad()
    {
        EventBad(1);
    }

    private void EventBad(int count)
    {
        _eventBad+=count;
        Debug.Log($"Bad++: {_eventBad}");
    }

    private void EventAddToPool(GameObject prefab)
    {
        if (!_poolPrefabs.Contains(prefab))
        {
            Debug.Log($"Adding event {prefab.name} to pool.");
            _poolPrefabs.Add(prefab);
        }
    }

    private void EventCreateThing(Thing thing, GameObject prefab)
    {
        Thing newThing = CreateThing(prefab);
        newThing.transform.position = thing.transform.position;
        BumpSortOrder(newThing);
    }

    private void EventCreateThings(Thing thing, InitThingsScriptableObject initThingsScriptableObject)
    {
        foreach (InitThing initThing in initThingsScriptableObject.InitThings)
        {
            Thing newThing = InitThing(initThing);
            if (newThing)
            {
                TweenThingToPosition(newThing, initThing.Position);
                BumpSortOrder(newThing);
            }
        }
    }

    private void EventInit(InitThingsScriptableObject initThingsScriptableObject)
    {
        foreach (InitThing initThing in initThingsScriptableObject.InitThings)
        {
            Thing newThing = InitThing(initThing);
            if (newThing)
            {
                TweenThingToPosition(newThing, initThing.Position);
                BumpSortOrder(newThing);
            }
        }
    }

    private void EventRemove(Thing thing)
    {
        _spawnedThings.Remove(thing);
        if (_randomThings.Contains(thing))
        {
            _randomThings.Remove(thing);
        }
        Destroy(thing.gameObject);
    }

    private void GrabStart(Thing thing)
    {
        _grabbedThing = thing;
        _inactiveDelta = 0;
        Debug.Log($"GrabStart: {thing}");
        thing.transform.rotation = Quaternion.Euler(0f,0f,0f);
        BumpSortOrder(thing);
    }

    private void BumpSortOrder(Thing thing)
    {
        _currentSortingOrder++;
        thing.SetSpriteRendererSortingOrder(_currentSortingOrder);
    }

    private void GrabEnd(Thing thing)
    {
        _grabbedThing = null;
        _inactiveDelta = 0;
        List<Thing> findThings = FindIntersectingThings(thing);
        Thing foundThing = GetClosestThing(findThings);
        if (foundThing != null)
        {
            Debug.Log($"GrabEnd: {thing} -> {foundThing}");
            CombineThingEvent combineThingEvent = thing.GetCombineThingEvent(foundThing.Prefab);
            if (combineThingEvent != null)
            {
                thing.SetLastCombinedEvent(foundThing);
                combineThingEvent.UnityEvent.Invoke();
            }
        }
        else
        {
            Debug.Log($"GrabEnd: {thing}");
        }
    }

    private List<Thing> FindIntersectingThings(Thing thing)
    {
        List<Thing> findThings = new List<Thing>();
        foreach (Thing otherThing in _spawnedThings)
        {
            if (thing != otherThing && thing.BoxCollider2D.bounds.Intersects(otherThing.BoxCollider2D.bounds))
            {
                findThings.Add(otherThing);
            }
        }
        return findThings;
    }

    private Thing GetClosestThing(List<Thing> findThings)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Thing closestThing = null;
        float closestDistance = float.MaxValue;
        foreach (Thing findThing in findThings)
        {
            float distance = Vector2.Distance(mousePos, findThing.BoxCollider2D.ClosestPoint(mousePos));
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestThing = findThing;
            }
        }
        return closestThing;
    }

    public void Use(Thing thing)
    {
        Debug.Log($"Use: {thing}");
        thing.GetUseThingEvent().Invoke();
    }

}
