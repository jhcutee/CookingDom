using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Spoon : ItemDraggableToPanBase
{
    [SerializeField] private GameObject tomato;
    [SerializeField] private TomatoBox box;
    private bool isFilled = false;
    public override void OnMouseDrag()
    {
        base.OnMouseDrag();
        
        if (!isFilled)
        {
            if (TryHitTomatoBox())
            {
                isFilled = true;
                tomato.SetActive(true);
                box.ShakeTheBox();
            }
        }
    }
    public override void OnMouseUp()
    {
        if (!dragging) return;
        dragging = false;


        if (TryHitPan() && isFilled == true && ActionDone == false && PanController.instance.CanDropIntoPan(this.gameObject))
        {
            if (pourAnchor)
                transform.DOMove(pourAnchor.position, moveToAnchorTime).OnComplete(PlayPour);
            else
                PlayPour();
        }
        else
        {
            // không trúng nồi thì đưa về chỗ cũ (nhẹ nhàng)
            transform.DOMove(startPos, 0.15f);
            spriteRenderer.sortingOrder = originalSortingOrder;
        }
    }
    private bool TryHitTomatoBox()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, 0.25f);
        for (int i = 0; i < hits.Length; i++)
            if (hits[i].GetComponent<TomatoBox>())
                return true;
        return false;
    }
    public override IEnumerator IEPlayPour()
    {
        animator.SetTrigger("Pour");
        yield return new WaitForSeconds(0.1666666f);
        tomato.SetActive(false);
        yield return new WaitForSeconds(1f);
        PourFinished();
    }
}
