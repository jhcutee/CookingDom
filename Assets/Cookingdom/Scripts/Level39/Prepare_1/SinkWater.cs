using UnityEngine;

public class SinkWater : MonoBehaviour
{
    public Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void Drainage()
    {
        if (!this.gameObject.activeInHierarchy) return;
            animator.SetTrigger("Drainage");
    }
    public void SelfSetActive()
    {
        gameObject.SetActive(false);
    }
}
