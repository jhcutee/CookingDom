using UnityEngine;

public class SinkController : MonoBehaviour
{
    public static SinkController instance;
    [Header("elements")]
    [SerializeField] private Knob knob;
    [SerializeField] private PlugSocket plugSocket;
    [SerializeField] private SinkWater SinkWater;

    private bool isPlugged;
    public bool IsPlugged => isPlugged;
    public bool IsWaterOn => knob.isOn && isPlugged;
    private void Awake()
    {
        instance = this;
    }
    private void OnEnable()
    {
        knob.turnOn += FillTheSink;
        plugSocket.OnPlugged += FillTheSink;

    }
    private void OnDisable()
    {

        knob.turnOn -= FillTheSink;
        plugSocket.OnPlugged -= FillTheSink;
    }
    public void FillTheSink(bool isPlugged)
    {
        this.isPlugged = isPlugged;
        if (isPlugged && knob.isOn)
        {
            SinkWater.gameObject.SetActive(true);
        }
        else if(!isPlugged)
        {
            SinkWater.Drainage();
        }
    }
    public void FillTheSink()
    {
        if (isPlugged && knob.isOn)
        {
            SinkWater.gameObject.SetActive(true);
        }
    }
}
