using System;
using UnityEngine;

public class HarvestStep : MonoBehaviour, ILevelStep
{
    [Header("Settings")]
    [SerializeField] private int requireCount = 4;

    private StepContext sct;
    private Spot[] spots;
    private int done;
    public event Action<ILevelStep> Completed;
   

    public void Begin(StepContext sct)
    {
        this.sct = sct;

        spots = GetComponentsInChildren<Spot>(true);

        foreach (var s in spots) s.HarvestArrived += OnArrived;
    }

    public void End()
    {
        if (spots != null)
            foreach (var s in spots) if (s) s.HarvestArrived -= OnArrived;
    }
    void OnArrived(Spot s)
    {
        done++;
        if (done >= requireCount) {
            Completed?.Invoke(this);
        }
        ;
    }
}
