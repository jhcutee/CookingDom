using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class DraggableItem : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] protected List<Transform> snapTargets = new();
    [SerializeField] protected List<SortingOrderHelper> renderers = new();
    public Transform lastSnappedTarget { get; private set; }

    [Header("Settings")]
    public bool draggable = true;
    public bool keepZ = true;
    public float snapThreshold = 0.75f;
    public bool snapToTargetPos = true;

    [Header("Tween Settings")]
    public float revertDuration = 0.18f;
    public float snapDuration = 0.12f;
    public bool useDOTween = true;

    [Header("Events")]
    public UnityEvent onPick;
    public UnityEvent onDropInvalid;
    [System.Serializable] public class DropToTargetEvent : UnityEvent<Transform> { }
    public DropToTargetEvent OnDropValidTo;

    protected Camera mainCam;
    protected Vector3 startPos;
    protected float startZ;
    protected bool dragging = false;
    protected Vector3 dragOffset;

    protected int settingSortingOrder = 100;

    void Awake()
    {
        mainCam = Camera.main;
        Physics2D.queriesHitTriggers = true;
        CacheDefaultSortingOrder();
    }

    void OnDisable()
    {
        // Phòng trường hợp object bị disable giữa chừng, trả lại default tránh “kẹt” sort cao.
        ReturnDefaultSortingOrder();
        // Đồng thời hủy mọi tween đang chạy để không gọi callback khi đã disable.
        DOTween.Kill(transform);
    }

    // ----- Sorting helpers -----
    void CacheDefaultSortingOrder()
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i].renderer != null)
            {
                var sr = renderers[i].renderer.GetComponent<SpriteRenderer>();
                if (sr) renderers[i].defaultSortingOrder = sr.sortingOrder;
            }
        }
    }

    void ReturnDefaultSortingOrder()
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i].renderer != null)
            {
                var sr = renderers[i].renderer.GetComponent<SpriteRenderer>();
                if (sr) sr.sortingOrder = renderers[i].defaultSortingOrder;
            }
        }
        settingSortingOrder = 100;
    }

    void RaisePickedSortingOrders()
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            var go = renderers[i].renderer;
            if (go && go.activeInHierarchy)
            {
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr)
                {
                    sr.sortingOrder = settingSortingOrder;
                    settingSortingOrder--;
                }
            }
        }
    }

    void NotifyPickedUpFromBoardIfAny()
    {
        var onBoard = GetComponent<ItemBroadPlaceableBase>();
        if (onBoard != null)
        {
            BroadTarget.instance?.OnItemPickedUpFromBoard();
        }
    }

    // ----- Mouse -----
    void OnMouseDown()
    {
        if (!draggable) return;

        startPos = transform.position;
        startZ = transform.position.z;

        var mw = ScreenToWorld(Input.mousePosition);
        dragOffset = transform.position - mw;

        dragging = true;
        RaisePickedSortingOrders();
        NotifyPickedUpFromBoardIfAny();
        onPick?.Invoke();
    }

    void OnMouseDrag()
    {
        if (!dragging) return;

        var mw = ScreenToWorld(Input.mousePosition);
        var pos = mw + dragOffset;
        if (keepZ) pos.z = startZ;
        transform.position = pos;

        Physics2D.SyncTransforms();
    }

    protected virtual void OnMouseUp()
    {
        if (!dragging) return;
        dragging = false;

        var (best, dist) = FindNearestAcceptableTarget(transform.position);
        bool valid = best && dist <= snapThreshold;
        lastSnappedTarget = valid ? best : null;

        if (valid)
        {
            OnDropValidTo?.Invoke(best);

            var tBase = best.GetComponent<DropTargetBase>();
            if (tBase != null) tBase.OnItemDropped(this);

            if (snapToTargetPos)
            {
                Vector3 dst = ResolveSnapDestination(best, tBase);   // <— dùng hàm mới

                if (useDOTween)
                {
                    var seq = DOTween.Sequence().SetTarget(transform);
                    seq.Join(transform.DOMove(dst, snapDuration).SetEase(Ease.OutQuad));

                    if (tBase != null && tBase.TryGetRotationZ(out float z))
                        seq.Join(transform.DORotate(new Vector3(0, 0, z), snapDuration, RotateMode.Fast));

                    seq.OnComplete(ReturnDefaultSortingOrder);
                }
                else
                {
                    StartCoroutine(CoMove(transform.position, dst, snapDuration, ReturnDefaultSortingOrder));
                }
            }
            else
            {
                ReturnDefaultSortingOrder();
            }
        }
        else
        {
            onDropInvalid?.Invoke();

            if (useDOTween)
            {
                transform.DOMove(KeepZ(startPos), revertDuration)
                         .SetEase(Ease.InOutQuad)
                         .SetTarget(transform)
                         .OnComplete(ReturnDefaultSortingOrder);
            }
            else
            {
                StartCoroutine(CoMove(transform.position, KeepZ(startPos), revertDuration, ReturnDefaultSortingOrder));
            }
        }
    }
    protected virtual Vector3 ResolveSnapDestination(Transform best, DropTargetBase tBase)
    {
        if (tBase != null)
            return KeepZ(tBase.GetSnapWorldPosition(this));
        return KeepZ(best.position);
    }
    // ----- Target pick -----
    (Transform, float) FindNearestAcceptableTarget(Vector3 pos)
    {
        Transform best = null;
        float bestD = float.PositiveInfinity;
        if (snapTargets == null || snapTargets.Count == 0) return (null, bestD);

        foreach (var t in snapTargets)
        {
            if (!t) continue;
            var baseT = t.GetComponent<DropTargetBase>();
            bool allowed = (baseT == null) || baseT.CanAccept(this);
            if (!allowed) continue;

            float d = Vector2.Distance(pos, t.position);
            if (d < bestD) { bestD = d; best = t; }
        }
        return (best, bestD);
    }

    // ----- Utils -----
    Vector3 ScreenToWorld(Vector3 screenPos)
    {
        if (!mainCam) return screenPos;
        float z = Mathf.Abs(mainCam.transform.position.z);
        return mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, z));
    }

    Vector3 KeepZ(Vector3 v)
    {
        if (keepZ) v.z = startZ;
        return v;
    }

    IEnumerator CoMove(Vector3 a, Vector3 b, float dur, System.Action onComplete = null)
    {
        if (dur <= 0f)
        {
            transform.position = b;
            onComplete?.Invoke();
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            transform.position = Vector3.Lerp(a, b, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        transform.position = b;
        onComplete?.Invoke();
    }
}

[System.Serializable]
public class SortingOrderHelper
{
    public GameObject renderer;
    public int defaultSortingOrder;
}
