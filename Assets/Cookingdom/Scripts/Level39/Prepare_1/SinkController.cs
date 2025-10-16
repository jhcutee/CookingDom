using UnityEngine;

public class SinkController : MonoBehaviour
{
    public static SinkController instance;

    [Header("Elements")]
    [SerializeField] private Knob knob;
    [SerializeField] private SinkWater sinkWater;
    public SinkWater SinkWater => sinkWater;
    [SerializeField] private SinkTarget sinkTarget;

    [Header("Plug Targets")]
    [SerializeField] private Transform drainTarget;
    [SerializeField] private Transform restTarget;

    private bool isPlugged;

    public bool IsPlugged => isPlugged;
    public bool IsWaterOn => knob && knob.isOn;

    void Awake() { instance = this; }

    void OnEnable()
    {
        if (knob) knob.turnOn += OnFaucetChanged;
    }

    void OnDisable()
    {
        if (knob) knob.turnOn -= OnFaucetChanged;
    }


    void OnFaucetChanged(bool on)
    {
        if (on && isPlugged)
        {
            if (!sinkWater.gameObject.activeSelf)
            { 
                sinkWater.gameObject.SetActive(true); 
                if(sinkTarget.isOccupied){
                    var beetrootItem = FindAnyObjectByType<BeetrootItem>();
                    if(beetrootItem) beetrootItem.WashBeetroot();
                }
            }

        }
    }
    public void WashBeetroot()
    {
        
    }

    //UnityEvents
    public void OnPlugPicked()
    {
        SetPlugged(false);
    }

    //UnityEvent DropValidTo
    public void OnPlugDroppedTo(Transform target)
    {
        if (!target) { 
            SetPlugged(false); 
            return; 
        }

        if (target == drainTarget) { 
            SetPlugged(true); 
        }
        else SetPlugged(false);
    }

    
    public void OnPlugDropInvalid()
    {
        SetPlugged(false);
    }


    void SetPlugged(bool plugged)
    {
        if (isPlugged == plugged) return;
        isPlugged = plugged;

        if (isPlugged)
        {

            if (IsWaterOn && !sinkWater.gameObject.activeSelf)
                sinkWater.gameObject.SetActive(true);
        }
        else
        {

            if (sinkWater.gameObject.activeSelf)
                sinkWater.Drainage();
        }
    }
}
