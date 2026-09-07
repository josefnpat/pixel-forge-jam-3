using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThingManager : MonoBehaviour
{
    private int _eventGood = 0;
    private int _eventBad = 0;
    private int _currentSortingOrder = 0;

    private Dictionary<GameObject, int> _thingInitCounts = new Dictionary<GameObject, int>();

    [SerializeField]
    private List<GameObject> _poolPrefabs;

    [SerializeField]
    private InitThingsScriptableObject _initThings;

    [SerializeField]
    private GameObject _moneyPrefab;

    private List<Thing> _spawnedThings = new List<Thing>();
    private List<Thing> _randomThings = new List<Thing>();

    private float _randomThingSpawnDelta = 1;
    private float _randomThingSpawnMax = 4;
    private float _randomThingSpawnTimeMin = 1;
    private float _randomThingSpawnTimeMax = 3;
    private float _offscreenPositionY = 2f;
    private float _randomPositionXMin = -0.25f;
    private float _randomPositionXMax = 0.25f;
    private float _randomPositionYMin = -0.25f;
    private float _randomPositionYMax = 0.25f;

    private float _gameTime;
    public float GameTime { get { return _gameTime; } }

    public void Start()
    {
        System.DateTimeOffset utcNow = System.DateTimeOffset.UtcNow;
        long utcUnixSeconds = utcNow.ToUnixTimeSeconds();
        long offsetSeconds = (long)System.TimeZoneInfo.Local.GetUtcOffset(utcNow).TotalSeconds;
        _gameTime = utcUnixSeconds + offsetSeconds;
        foreach (InitThing initThing in _initThings.InitThings)
        {
            InitThing(initThing);
        }
    }

    public void Update()
    {
        _gameTime += Time.deltaTime;
        _randomThingSpawnDelta -= Time.deltaTime;
        if (_randomThingSpawnDelta <= 0)
        {
            if (_randomThings.Count < _randomThingSpawnMax)
            {
                _randomThingSpawnDelta = Random.Range(_randomThingSpawnTimeMin, _randomThingSpawnTimeMax);
                InitThingRandom();
            }
        }
    }

    private void InitThingRandom()
    {
        List<GameObject> thresholdPoolPrefabs = ThresholdPoolPrefabs();
        if (thresholdPoolPrefabs.Count > 0)
        {
            Thing thing = InitThingFromOffscreen(thresholdPoolPrefabs[Random.Range(0,thresholdPoolPrefabs.Count)]);
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

    private List<GameObject> ThresholdPoolPrefabs()
    {
        List<GameObject> thresholdPoolPrefabs = new List<GameObject>();
        foreach (GameObject prefab in _poolPrefabs)
        {
            Thing thing = prefab.GetComponent<Thing>();
            if (thing.WithinThreshold(_eventGood, _eventBad))
            {
                thresholdPoolPrefabs.Add(prefab);
            }
        }
        Debug.Log($"There are {_poolPrefabs.Count} things in the pool and {thresholdPoolPrefabs.Count} of them are within threshold.");
        return thresholdPoolPrefabs;
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

    private Thing InitThing(GameObject prefab)
    {
        if (!_thingInitCounts.ContainsKey(prefab))
        {
            _thingInitCounts[prefab] = 0;
        }
        int count = _thingInitCounts[prefab];
        if (prefab.GetComponent<Thing>().CanInit(count))
        {
            _thingInitCounts[prefab]++;

            GameObject go = Instantiate(prefab);
            Thing thing = go.GetComponent<Thing>();
            thing.Prefab = prefab;
            
            thing.OnEventAddToPool.AddListener(EventAddToPool);
            thing.OnEventCreateThing.AddListener(EventCreateThing);
            thing.OnEventCreateThings.AddListener(EventCreateThings);
            thing.OnEventInit.AddListener(EventInit);
            thing.OnEventRemove.AddListener(EventRemove);
            thing.OnEventGood.AddListener(EventGood);
            thing.OnEventBad.AddListener(EventBad);
            thing.OnEventEarnMoney.AddListener(EventEarnMoney);
            thing.OnGrabStart.AddListener(GrabStart);
            thing.OnGrabEnd.AddListener(GrabEnd);
            thing.OnUse.AddListener(Use);
            
            _spawnedThings.Add(thing);
            BumpSortOrder(thing);
            
            return thing;
        }

        return null;
    }

    private void EventEarnMoney(int value)
    {
        for (int i = 0; i < value; i++)
        {
            InitThingFromOffscreen(_moneyPrefab);
        }
    }

    private void EventGood()
    {
        _eventGood++;
        Debug.Log($"Good++: {_eventGood}");
    }

    private void EventBad()
    {
        _eventBad++;
        Debug.Log($"Bad++: {_eventBad}");
    }

    private void EventAddToPool(GameObject prefab)
    {
        if (!_poolPrefabs.Contains(prefab))
        {
            _poolPrefabs.Add(prefab);
        }
    }

    private void EventCreateThing(Thing thing, GameObject prefab)
    {
        Thing newThing = InitThing(prefab);
        if (newThing)
        {
            newThing.transform.position = thing.transform.position;
            BumpSortOrder(newThing);
        }
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
