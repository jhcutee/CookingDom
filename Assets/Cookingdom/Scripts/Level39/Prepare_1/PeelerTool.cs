using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PeelerTool : MonoBehaviour
{
    public Transform cuttingBoard;       // chỉ nạo khi quả đang đặt trên thớt này
    public float shootInterval = 0.25f;  // chu kỳ bắn ray
    public float chooseRadius = 0.2f;   // chọn quả gần điểm chuột trong bán kính nhỏ

    Camera cam;
    bool sessionActive;
    float nextShootTime;
    Vector3 lastWorld;
    PeelableItem current;

    void Awake()
    {
        cam = Camera.main;
        Physics2D.queriesHitTriggers = true;
    }

    void OnMouseDown()
    {
        sessionActive = true;
        nextShootTime = Time.time;
        lastWorld = ToWorld(Input.mousePosition);

        current = FindPeelableAt(lastWorld);
        if (current != null) current.BeginSession();
    }

    void OnMouseUp()
    {
        sessionActive = false;
        if (current != null) current.EndSession();
        current = null;
    }

    void Update()
    {
        if (!sessionActive) return;
        if (Time.time < nextShootTime) return;

        nextShootTime += Mathf.Max(0.01f, shootInterval);

        Vector3 w = ToWorld(Input.mousePosition);
        Vector2 dir = (w - lastWorld);

        var target = FindPeelableAt(w);
        if (target != current)
        {
            if (current != null) current.EndSession();
            current = target;
            if (current != null) current.BeginSession();
        }

        if (current != null)
            current.TrySpawnStamp(w, dir);

        lastWorld = w;
    }

    PeelableItem FindPeelableAt(Vector3 worldPos)
    {
        PeelableItem best = null;

        var hit = Physics2D.OverlapPoint((Vector2)worldPos);
        if (hit) best = hit.GetComponentInParent<PeelableItem>();

        if (best == null && chooseRadius > 0f)
        {
            var hits = Physics2D.OverlapCircleAll(worldPos, chooseRadius);
            float bestD = float.PositiveInfinity;
            foreach (var h in hits)
            {
                var p = h ? h.GetComponentInParent<PeelableItem>() : null;
                if (!p) continue;
                float d = Vector2.Distance(worldPos, p.transform.position);
                if (d < bestD) { best = p; bestD = d; }
            }
        }

        if (best != null && cuttingBoard != null)
        {
            var d = best.GetComponent<DraggableItem>();
            if (!d || d.lastSnappedTarget != cuttingBoard) best = null;
        }
        return best;
    }

    Vector3 ToWorld(Vector2 screen)
    {
        float z = Mathf.Abs(cam ? cam.transform.position.z : 10f);
        return cam ? cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, z))
                   : new Vector3(screen.x, screen.y, 0f);
    }
}
