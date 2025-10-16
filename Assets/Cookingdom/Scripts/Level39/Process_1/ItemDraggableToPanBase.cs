using DG.Tweening;
using System.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;

public class ItemDraggableToPanBase : MonoBehaviour
{
    [Header("Drag")] protected bool ActionDone = false;
    [SerializeField] protected float followSpeed = 25f;

    [Header("Pot Detect")]
    [SerializeField] protected string panTag = "Pot";
    [SerializeField] protected float hitRadius = 0.25f;

    [SerializeField] protected Animator animator;
    [SerializeField] protected Transform pourAnchor;
    [SerializeField] protected GameObject panEffect;
    [SerializeField] protected SpriteRenderer spriteRenderer; 

    [Header("Tween")]
    [SerializeField] protected float moveToAnchorTime = 0.12f;
    [SerializeField] protected float returnTime = 0.25f;


    protected Camera cam;
    protected bool dragging;
    protected Vector3 grabOffset;
    protected Vector3 startPos;
    protected int originalSortingOrder;
    public bool canDrag = true;
    protected virtual void Awake()
    {
        cam = Camera.main;
        startPos = transform.position;
        if (!animator) animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(!spriteRenderer)  originalSortingOrder = spriteRenderer.sortingOrder;
    }

    protected virtual void OnMouseDown()
    {
        if (!canDrag) return;
        var m = GetMouseWorld();
        grabOffset = transform.position - m;
        dragging = true;
        spriteRenderer.sortingOrder = 100;
    }

    public virtual void OnMouseDrag()
    {
        if (!canDrag) return;
        if (!dragging) return;
        var m = GetMouseWorld() + grabOffset;
        m.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, m, Time.deltaTime * followSpeed);
    }

    public virtual void OnMouseUp()
    {
        if (!canDrag) return;
        if (!dragging) return;
        dragging = false;


        if (TryHitPan() && !ActionDone && PanController.instance.CanDropIntoPan(this.gameObject))
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
    public void SetCanDrag(bool value) => ActionDone = value;

    protected bool TryHitPan()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, hitRadius);
        for (int i = 0; i < hits.Length; i++)
            if (hits[i].CompareTag(panTag))
                 return true;
        return false;
    }
    Vector3 GetMouseWorld()
    {
        var w = cam.ScreenToWorldPoint(Input.mousePosition);
        w.z = transform.position.z;
        return w;
    }
    public void PlayPour()
    {
        StartCoroutine(IEPlayPour());
    }
    public virtual IEnumerator IEPlayPour()
    {
        animator.SetTrigger("Pour");
        yield return new WaitForSeconds(1f);
        PourFinished();
    }
    public void PourFinished()
    {
        transform.DOMove(startPos, returnTime).OnComplete(() =>
        {
            ActionDone = true; // chỉ cho đổ 1 lần
            spriteRenderer.sortingOrder = originalSortingOrder;
        });
    }
    public void AE_ShowPanEffect()
    {
        if (panEffect) panEffect.SetActive(true);
    }
}
