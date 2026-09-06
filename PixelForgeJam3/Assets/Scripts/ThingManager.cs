using System.Collections.Generic;
using UnityEngine;

public class ThingManager : MonoBehaviour
{

    private int _currentSortingOrder = 0;

    [SerializeField]
    private List<GameObject> _poolPrefabs;

    [SerializeField]
    private List<InitThing> _initPrefabs;

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

    public void Start()
    {
        foreach (InitThing initThing in _initPrefabs)
        {
            InitThing(initThing);
        }
    }

    public void Update()
    {
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

    private Thing InitThingRandom()
    {
        Thing thing = InitThing(_poolPrefabs[Random.Range(0,_poolPrefabs.Count)]);
        _randomThings.Add(thing);
        float x = Random.Range(_randomPositionXMin,_randomPositionXMax);
        float y = Random.Range(_randomPositionYMin,_randomPositionYMax);
        thing.transform.position = new Vector3(x,_offscreenPositionY,0);
        thing.TweenTo(new Vector3(x,y,0));
        thing.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        return thing;
    }

    private Thing InitThing(InitThing initThing)
    {
        Thing newThing = InitThing(initThing.Prefab);
        newThing.transform.position = initThing.Position;
        return newThing;
    }

    private Thing InitThing(GameObject prefab)
    {
        GameObject go = Instantiate(prefab);
        Thing thing = go.GetComponent<Thing>();
        thing.Prefab = prefab;
        
        thing.OnEventCreate.AddListener(EventCreate);
        thing.OnEventRemove.AddListener(EventRemove);
        thing.OnGrabStart.AddListener(GrabStart);
        thing.OnGrabEnd.AddListener(GrabEnd);
        thing.OnUse.AddListener(Use);
        _spawnedThings.Add(thing);
        BumpSortOrder(thing);
        return thing;
    }

    private void EventCreate(Thing thing, GameObject prefab)
    {
        Thing newThing = InitThing(prefab);
        newThing.transform.position = thing.transform.position;
        BumpSortOrder(newThing);
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
        Thing foundThing = GetTopThing(findThings);
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

    private Thing GetTopThing(List<Thing> findThings)
    {
        Thing foundThing = null;
        int topThingSortOrder = int.MinValue;
        foreach (Thing findThing in findThings)
        {
            if (foundThing == null)
            {
                foundThing = findThing;
            }
            else
            {
                int touchingThingSortOrder = findThing.GetSpriteRendererSortingOrder();
                if (touchingThingSortOrder > topThingSortOrder)
                {
                    foundThing = findThing;
                    topThingSortOrder = touchingThingSortOrder;
                }
            }
        }
        return foundThing;
    }

    public void Use(Thing thing)
    {
        Debug.Log($"Use: {thing}");
        thing.GetUseThingEvent().Invoke();
    }

}
