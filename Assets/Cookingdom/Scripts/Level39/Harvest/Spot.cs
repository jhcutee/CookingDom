using System;
using UnityEngine;

public class Spot : MonoBehaviour
{
    public bool IsHarvested { get; private set; }
    [Header("Elements")]
    [SerializeField] private Beetroot beetroot;
    [SerializeField] private LeafPullHandle pullHandle;
    [Header("Settings")]
    [SerializeField] private string harvestedTrigger = "Harvested";

    public event Action<Spot> HarvestArrived;
    private void Awake()
    {
        beetroot = GetComponentInChildren<Beetroot>();
        pullHandle = GetComponentInChildren<LeafPullHandle>();
    }
    public void Harvest()
    {
        if (IsHarvested) return;
        IsHarvested = true;
        beetroot.Animator?.SetTrigger(harvestedTrigger);
    }
    public void StopHarvest()
    {
        if (!IsHarvested) return;
        IsHarvested = false;
        beetroot.Animator?.ResetTrigger(harvestedTrigger);
    }
    public void AnimEvent_HarvestArrived() => HarvestArrived?.Invoke(this);
}
