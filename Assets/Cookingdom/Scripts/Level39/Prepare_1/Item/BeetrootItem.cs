using UnityEngine;

public class BeetrootItem : ItemBroadPlaceableBase
{
    [Header("Elements")]
    [SerializeField] Animator animator;
    [SerializeField] private GameObject dust;
    public bool isWashed = false;
   
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void WashBeetroot()
    {
        if(!isWashed)
        {
            animator.SetTrigger("Wash");    
        }
    }
    public void AE_WashDone()
    {
        isWashed = true;
        dust.SetActive(false);
    }

    public override void OnBoardPlaced()
    {
        animator.SetTrigger("BoardPlaced");
    }

}
