using DG.Tweening;
using UnityEngine;

public class PanDragDrop : ItemDraggableToPanBase
{
    [SerializeField] private GameObject soupInPlate;
    [SerializeField] private GameObject soupInPan;
    public Collider2D selCollider;
    protected override void Awake()
    {
        cam = Camera.main;
        startPos = transform.position;
        if (!animator) animator = GetComponent<Animator>();
        if (!spriteRenderer) originalSortingOrder = spriteRenderer.sortingOrder;
        selCollider = GetComponent<Collider2D>();
        selCollider.enabled = false;
        canDrag = false;
    }
    public override void OnMouseUp()
    {
        if (!canDrag) return;
        if (!dragging) return;
        dragging = false;


        if (TryHitPan())
        {

            soupInPlate.SetActive(true);
            soupInPan.SetActive(false);
        }
        transform.DOMove(startPos, 0.15f);
        spriteRenderer.sortingOrder = originalSortingOrder;
       
    }
}
