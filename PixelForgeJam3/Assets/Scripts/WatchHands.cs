using UnityEngine;

public class WatchHands : MonoBehaviour
{

    [SerializeField]
    private Transform HourTransform;
    [SerializeField]
    private Transform MinuteTransform;

    private ThingManager _thingManager;
    private Thing _thing;
    private float _currentTime;

    public void Start()
    {
        _thingManager = FindFirstObjectByType<ThingManager>();
        _thing = GetComponent<Thing>();
        _thing.OnUse.AddListener(UseWatch);
        _currentTime = _thingManager.GameTime + UnityEngine.Random.Range(0,12*60*60);
    }

    public void Update()
    {
        _currentTime += Time.deltaTime;
        System.DateTimeOffset dateTimeOffset = System.DateTimeOffset.FromUnixTimeSeconds((long)_currentTime);
        int minute = dateTimeOffset.Minute;
        int hour24 = dateTimeOffset.Hour;
        int hour12 = hour24 % 12 == 0 ? 12 : hour24 % 12;

        MinuteTransform.transform.rotation = transform.rotation * Quaternion.Euler(
            0f,
            0f,
            -(float)minute / 60 * 360f
        );

        HourTransform.transform.rotation = transform.rotation * Quaternion.Euler(
            0f,
            0f,
            -(float)hour12 / 12 * 360f
        );

    }

    private void UseWatch(Thing thing)
    {
        _currentTime = _thingManager.GameTime;
    }

}
