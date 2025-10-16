using UnityEngine;

public class BellPepperItem : ItemBroadPlaceableBase
{
    Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public override void OnBoardPlaced()
    {
        animator.SetTrigger("BoardPlaced");
    }
}
