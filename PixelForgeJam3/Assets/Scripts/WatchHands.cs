using UnityEngine;

public class WatchHands : MonoBehaviour
{

    [SerializeField]
    private Transform _hourTransform;
    [SerializeField]
    private Transform _minuteTransform;
    [SerializeField]
    private Transform _secondTransform;
    [SerializeField]
    private bool _timeNeedsToBeSet = false;
    [SerializeField]
    private bool _deadBattery = false;

    private ThingManager _thingManager;
    private Thing _thing;
    private double _offsetTime;

    public void Start()
    {
        _thingManager = FindFirstObjectByType<ThingManager>();
        _thing = GetComponent<Thing>();
        _thing.OnUse.AddListener(UseWatch);
        if (_timeNeedsToBeSet)
        {
            _offsetTime = UnityEngine.Random.Range(0,12*60*60);
        }
    }

    public void Update()
    {
        double watchTime = _thingManager.GameTime + _offsetTime;
        System.DateTimeOffset dateTimeOffset = System.DateTimeOffset.FromUnixTimeSeconds((int)watchTime);
        int second = dateTimeOffset.Second;
        int minute = dateTimeOffset.Minute;
        int hour24 = dateTimeOffset.Hour;
        int hour12 = hour24 % 12 == 0 ? 12 : hour24 % 12;

        _secondTransform.transform.rotation = transform.rotation * Quaternion.Euler(
            0f,
            0f,
            -(float)second / 60 * 360f
        );

        _minuteTransform.transform.rotation = transform.rotation * Quaternion.Euler(
            0f,
            0f,
            -(float)minute / 60 * 360f
        );

        _hourTransform.transform.rotation = transform.rotation * Quaternion.Euler(
            0f,
            0f,
            -(float)hour12 / 12 * 360f
        );

    }

    private void UseWatch(Thing thing)
    {
        _offsetTime = 0;
    }

}
