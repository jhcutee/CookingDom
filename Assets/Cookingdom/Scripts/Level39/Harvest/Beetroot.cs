using UnityEngine;

public class Beetroot : MonoBehaviour
{
    private Animator animator;
    public Animator Animator => animator;

    private Spot spot;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        spot = GetComponentInParent<Spot>();
    }
    public void AnimEvent_Harvested() => spot?.AnimEvent_HarvestArrived();
}
