using DG.Tweening;
using UnityEngine;

public class WoodenSpoon : ItemDraggableToPanBase
{
    [Header("Foot (điểm ở giữa mặt rộng)")]
    [SerializeField] private Transform foot;          // Kéo 1 child vào giữa mặt thìa
    [SerializeField] private float footPadding = 0.02f;

    public bool isOnPan = false;

    // orbit của điểm foot
    float orbitRadius;
    float orbitAngleRad; // rad
    Quaternion fixedRotOnPan; // rotation sẽ giữ nguyên trong lúc khuấy

    protected override void OnMouseDown()
    {
        base.OnMouseDown();
        // hiệu ứng tùy bạn, không ảnh hưởng logic khóa Z:
        transform.DORotate(new Vector3(0, 0, 180f), 0.25f);
    }

    public override void OnMouseUp()
    {
        if (!dragging) return;
        dragging = false;

        if (TryHitPan() && PanController.instance.IsReadyToCook())
        {
            PanController.instance.PanTarget.SnapToAnchorIfAny(this.transform);

            InitOrbitOnPan();        // cache bán kính + góc + rotation hiện tại
            isOnPan = true;
            canDrag = false;
        }
        else
        {
            ReturnStartPos();
        }

        spriteRenderer.sortingOrder = originalSortingOrder;
    }

    public void ReturnStartPos()
    {
        transform.DOKill();
        transform.DOMove(startPos, 0.15f);
        transform.DORotate(new Vector3(0, 0, -90f), 0.15f);
        isOnPan = false;
        canDrag = true;
    }

    // ======= được PanController gọi khi người chơi khuấy =======
    public void ApplyOrbitDelta(float deltaDeg)
    {
        if (!isOnPan) return;

        orbitAngleRad += deltaDeg * Mathf.Deg2Rad;

        Vector2 center = PanController.instance.transform.position;
        float panR = PanController.instance.GetPanInnerRadius();
        float safeR = Mathf.Max(0.03f, panR - EstimateFootRadius() - footPadding);
        orbitRadius = Mathf.Min(orbitRadius, safeR);

        // Mục tiêu cho điểm foot trên quỹ đạo
        Vector2 targetFoot = center + new Vector2(Mathf.Cos(orbitAngleRad), Mathf.Sin(orbitAngleRad)) * orbitRadius;

        // Dời cả thìa sao cho foot trùng target, KHÔNG thay rotation
        Vector3 curFoot = foot ? foot.position : transform.position;
        transform.position += (Vector3)targetFoot - curFoot;

        // Khóa rotation Z: luôn giữ nguyên hướng dựng đứng khi vừa cắm vào nồi
        transform.rotation = fixedRotOnPan;
    }

    // ======= setup lần đầu khi cắm vào nồi =======
    void InitOrbitOnPan()
    {
        Vector2 center = PanController.instance.transform.position;
        Vector2 fp = foot ? (Vector2)foot.position : (Vector2)transform.position;

        orbitRadius = Vector2.Distance(fp, center);
        orbitAngleRad = Mathf.Atan2(fp.y - center.y, fp.x - center.x);

        float panR = PanController.instance.GetPanInnerRadius();
        float safeR = Mathf.Max(0.03f, panR - EstimateFootRadius() - footPadding);
        orbitRadius = Mathf.Min(orbitRadius, safeR);

        // Cache rotation hiện tại để cố định trong suốt quá trình khuấy
        fixedRotOnPan = transform.rotation;
    }

    float EstimateFootRadius()
    {
        // Lấy kích thước sprite (bao gồm con) → bán kính ước lượng phần mặt thìa
        var srs = GetComponentsInChildren<SpriteRenderer>(true);
        if (srs.Length > 0)
        {
            Bounds b = srs[0].bounds;
            for (int i = 1; i < srs.Length; i++) b.Encapsulate(srs[i].bounds);
            return Mathf.Max(b.extents.x, b.extents.y) * 0.5f;
        }
        return 0.05f;
    }
}
