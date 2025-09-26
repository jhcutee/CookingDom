using UnityEngine;

public class CleaningBeetroot : ItemBase
{
    private Animator animator;
    [SerializeField] GameObject dust;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void Cleaning()
    {
        animator.SetTrigger("Clean");
    }
    public void DestroyDust()
    {
        Destroy(dust);
    }
}
