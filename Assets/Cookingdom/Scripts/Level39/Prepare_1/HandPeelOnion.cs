using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OnionHandPeel : MonoBehaviour
{
    [Header("Onion Refs")]
    public Collider2D onionCollider;           // Collider của củ hành
    public SpriteRenderer onionSkinRenderer;   // Renderer của lớp vỏ (skin)
    public Transform onionRoot;                // Transform để rung (root onion); trống -> dùng skin/collider

    [Header("Half Visual")]
    public GameObject halfPeeledGO;            // GameObject con của SKIN (để hiện trạng thái 50%)
    public GameObject flesh;
    [Header("Board Gate (optional)")]
    public bool requireOnBoard = false;
    public Transform cuttingBoard;

    [Header("Gesture")]
    public float minMoveWorld = 0.03f;         // 2 điểm liên tiếp phải cách nhau tối thiểu
    public float minHitInterval = 0.2f;        // chặn spam: mỗi hit cách nhau ít nhất 0.2s

    [Header("Shake")]
    public float shakeDuration = 0.08f;
    public Vector3 shakeStrength = new Vector3(0.03f, 0.03f, 0f);
    public int shakeVibrato = 12;
    public float shakeRandomness = 90f;

    [Header("Progress")]
    public int hitsToHalf = 5;                 // đủ 5 hit -> bật halfPeeledGO
    public int hitsToRemove = 10;              // đủ 10 hit -> tắt skin (kết thúc)

    Camera cam;
    bool holding;
    Vector3 prevWorld;
    int hits;
    bool finished;
    float nextAllowedHitTime;

    void Awake()
    {
        cam = Camera.main;
        Physics2D.queriesHitTriggers = true;
        if (halfPeeledGO) halfPeeledGO.SetActive(false);
    }

    void OnMouseDown()
    {
        if (finished) return;
        holding = true;
        prevWorld = ScreenToWorld(Input.mousePosition);
    }

    void OnMouseUp() { holding = false; }

    void OnMouseDrag()
    {
        if (!holding || finished) return;

        var curr = ScreenToWorld(Input.mousePosition);
        if (Vector2.Distance(prevWorld, curr) < minMoveWorld)
        {
            prevWorld = curr;
            return;
        }

        bool hitPrev = HitOnion(prevWorld);
        bool hitCurr = HitOnion(curr);

        if (hitPrev || hitCurr)
            RegisterHit();

        prevWorld = curr;
    }

    bool HitOnion(Vector2 world)
    {
        if (!onionCollider) return false;

        if (requireOnBoard && cuttingBoard)
        {
            var drag = onionCollider.GetComponentInParent<DraggableItem>();
            if (!(drag && drag.lastSnappedTarget == cuttingBoard)) return false;
        }

        return onionCollider.OverlapPoint(world);
    }

    void RegisterHit()
    {
        if (finished) return;
        if (Time.time < nextAllowedHitTime) return; // throttle
        nextAllowedHitTime = Time.time + Mathf.Max(0.01f, minHitInterval);

        var t = onionRoot ? onionRoot
                          : (onionSkinRenderer ? onionSkinRenderer.transform : onionCollider.transform);
        if (t)
        {
            t.DOKill();
            t.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness, false, true);
        }

        hits = Mathf.Min(hits + 1, Mathf.Max(hitsToRemove, hitsToHalf));

        if (hits == hitsToHalf && halfPeeledGO)
            halfPeeledGO.SetActive(true);

        if (hits >= hitsToRemove)
            FinishPeel();
    }

    void FinishPeel()
    {
        finished = true;
        // Tắt SKIN → halfPeeledGO là con của skin sẽ biến mất theo
        if (onionSkinRenderer)
            onionSkinRenderer.gameObject.SetActive(false);

        this.gameObject.GetComponent<CuttableItem>().canCut = true;
        flesh.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
    }

    public void ResetOnion()
    {
        finished = false;
        hits = 0;
        nextAllowedHitTime = 0f;
        if (onionSkinRenderer) onionSkinRenderer.gameObject.SetActive(true);
        if (halfPeeledGO) halfPeeledGO.SetActive(false);
    }

    Vector3 ScreenToWorld(Vector2 s)
    {
        float z = Mathf.Abs(cam ? cam.transform.position.z : 10f);
        return cam ? cam.ScreenToWorldPoint(new Vector3(s.x, s.y, z))
                   : new Vector3(s.x, s.y, 0f);
    }
}
