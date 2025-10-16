using UnityEngine;
using DG.Tweening;

public class ClayPot : MonoBehaviour
{
    [Header("Element References")]
    [SerializeField] private Knob knob;
    [SerializeField] private GameObject water;
    public bool HasWater => water.activeSelf;

    [Header("Scale")]
    [SerializeField] float sinkScaleMultiplier = 0.75f;   // 1 -> 0.75 khi vào sink

    [Header("Jiggle")]
    [SerializeField] float jiggleDuration = 0.12f;
    [SerializeField] float jiggleAmount = 0.1f;      // biên độ “thạch” X/Y
    [SerializeField] int jiggleVibrato = 10;
    [SerializeField] float jiggleElasticity = 0.9f;

    Vector3 _baseScale;

    void Awake()
    {
        _baseScale = transform.localScale; // scale gốc (ví dụ 1,1,1)
    }

    void OnDisable()
    {
        transform.DOKill();
        knob.turnOn -= OnKnobOn;
    }

    private void OnEnable()
    {
        //Check Knob State
        knob.turnOn += OnKnobOn;
    }

    // 1) Thả vào Sink: set scale NGAY -> nhún thạch
    public void OnDropSink()
    {
        ApplyScaleImmediate(sinkScaleMultiplier);
        DoJiggle();

        OnKnobOn(knob.isOn);
    }

    // 2) Thả vào Gas Stove: set scale NGAY về 1 -> nhún thạch
    public void OnDropGasStove()
    {
        ApplyScaleImmediate(1f);
        DoJiggle();
        if (HasWater)
        {
            this.GetComponent<DraggableItem>().draggable = false;
        }
    }

    // 3) Nhún “thạch” (relative quanh scale hiện tại)
    public void DoJiggle()
    {
        // chặn tween cũ để nhún không bị chồng
        transform.DOKill(false);
        transform.DOPunchScale(new Vector3(jiggleAmount, jiggleAmount, 0f),
                               jiggleDuration, jiggleVibrato, jiggleElasticity);
    }

    // 4) Set scale ngay lập tức (không tween)
    public void ApplyScaleImmediate(float multiplier)
    {
        transform.DOKill(false);
        transform.localScale = _baseScale * multiplier;
    }

    private void OnKnobOn(bool isOn)
    {
        var item = GetComponent<DraggableItem>();
        if(item.lastSnappedTarget == null) return;
        if (knob.isOn && item.lastSnappedTarget.name == "Sink")
        {
            water.SetActive(true);
        }
    }
}
