using DG.Tweening;
using System.Collections;
using UnityEngine;
using static UnityEditor.Progress;

public class Pepper : MonoBehaviour
{
    [Header("Drag")]
    [SerializeField] private bool canDrag = true;
    [SerializeField] private float followSpeed = 25f;

    [Header("Pot Detect")]
    [SerializeField] private string potTag = "Pot";
    [SerializeField] private float hitRadius = 0.25f;

    [Header("Pour Setup")]
    [SerializeField] private GameObject pourEffect; // hạt tiêu rơi
    [SerializeField] private Transform pourAnchor;
    [SerializeField] private Transform pepperAnchor;
    // đặt sẵn trong scene
    [SerializeField] private Animator animator;      // clip có trigger "Pour"
    [SerializeField] private string pourTrigger = "Pour";

    [Header("Tween")]
    [SerializeField] private float moveToAnchorTime = 0.12f;
    [SerializeField] private float returnTime = 0.25f;

    private Camera cam;
    private bool dragging;
    private Vector3 grabOffset;
    private Vector3 startPos;

    void Awake()
    {
        cam = Camera.main;
        startPos = transform.position;
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void OnMouseDown()
    {
        if (!canDrag) return;
        var m = GetMouseWorld();
        grabOffset = transform.position - m;
        dragging = true;
    }

    void OnMouseDrag()
    {
        if (!dragging || !canDrag) return;
        var m = GetMouseWorld() + grabOffset;
        m.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, m, Time.deltaTime * followSpeed);
    }

    void OnMouseUp()
    {
        if (!dragging) return;
        dragging = false;

        if (TryHitPot())
        {
            // đưa tới anchor rồi trigger anim
            if (pourAnchor)
                transform.DOMove(pourAnchor.position, moveToAnchorTime).OnComplete(PlayPour);
            else
                PlayPour();
        }
        else
        {
            // không trúng nồi thì đưa về chỗ cũ (nhẹ nhàng)
            transform.DOMove(startPos, 0.15f);
        }
    }

    void PlayPour()
    {
        if(ClayPotController.Instance.ClayPot.HasWater == false)
        {
            transform.DOMove(startPos, returnTime);
            return;
        }
        animator?.SetTrigger(pourTrigger);
        StartCoroutine(HandlePourANim());
        // canDrag sẽ tắt sau khi anim kết thúc (Animation Event gọi AnimEvent_PourFinished)
    }
    private IEnumerator HandlePourANim()
    {
        yield return new WaitForSeconds(0.5f);
        this.transform.DOMove(pourAnchor.position - new Vector3(0, 0.2f, 0), 0.2f);
        pourEffect.SetActive(true);
        pourEffect.transform.DOMove(pepperAnchor.position,  0.5f);
        yield return new WaitForSeconds(0.2f);
        this.transform.DOMove(pourAnchor.position, 0.2f);
        yield return new WaitForSeconds(0.3f);
        ClayPotController.Instance.AddIngredientStep1(pourEffect);
        ClayPotController.Instance.OnIngredientDropped(pourEffect);
        ClayPotController.Instance.GetComponent<Collider2D>().enabled = false;
        yield return new WaitForSeconds(0.1f);
        AnimEvent_PourFinished();
    }
    // Gọi ở cuối clip "Pour" bằng Animation Event
    public void AnimEvent_PourFinished()
    {
        transform.DOMove(startPos, returnTime).OnComplete(() =>
        {
            canDrag = false; // chỉ cho đổ 1 lần
        });
    }

    public void SetCanDrag(bool value) => canDrag = value;

    bool TryHitPot()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, hitRadius);
        for (int i = 0; i < hits.Length; i++)
            if (hits[i].CompareTag(potTag))
                if (hits[i].GetComponent<DraggableItem>()?.lastSnappedTarget?.gameObject.name == "Gas Stove")
                    return true;
                else return false;
                    
        return false;
    }

    Vector3 GetMouseWorld()
    {
        var w = cam.ScreenToWorldPoint(Input.mousePosition);
        w.z = transform.position.z;
        return w;
    }
}
