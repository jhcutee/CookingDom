using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class PeelableItem : MonoBehaviour
{
    public Transform cuttingBoard;
    // Phần hiển thị
    public SpriteRenderer skin;      // vỏ: Visible Outside Mask
    public SpriteRenderer flesh;     // ruột: Visible Inside Mask

    public ParticleSystem peelFX;
    public float peelFxInterval = 0.5f;

    // “Đống vỏ” sau khi hoàn tất (tuỳ chọn)
    public GameObject scraps;
    // Tham số gameplay
    public float minDistanceBetweenStamps = 0.08f; // khoảng cách tối thiểu giữa 2 tem
    public float stampLength = 0.35f;              // chiều dài tem (world)
    public float stampWidth = 0.09f;              // chiều rộng tem (world)
    public float finishThreshold01 = 0.6f;         // ngưỡng hoàn tất
    public int maxStamps = 200;                  // giới hạn tem mượn
    public float fillFactor = 0.75f;               // hệ số phủ để bù chồng chéo

    [Header("DOTWEEN")]
    public bool playFinishFX = true;              // có chạy FX hay không
    public float jumpPower = 0.4f;               // độ cao nhảy (world units)
    public int jumpCount = 1;                  // số lần nhảy
    public float jumpDuration = 0.28f;              // thời lượng nhảy
    public float scrapsPopDuration = 0.18f;        // thời gian pop scraps
    public float scrapsPopFrom = 0.6f;

    // Trạng thái
    public bool IsPeeled { get; private set; }
    public float Progress01 => Mathf.Clamp01((float)_usedCount / Mathf.Max(1, _neededCount));

    // Nội bộ
    readonly List<SpriteMask> _borrowed = new();   // tem đang dùng (để trả về pool)
    int _usedCount;
    int _neededCount = 80;
    Vector3 _lastStampPos;
    bool _hasLast;

    Vector3 _scrapsOriginalScale;
    bool _cachedScrapsScale;

    void Reset()
    {
        if (flesh) flesh.enabled = true;
        if (skin) skin.enabled = true;
        if (scraps) scraps.SetActive(false);
    }

    void Start() => RecalculateNeededCount();

#if UNITY_EDITOR
    void OnValidate() { if (!Application.isPlaying) RecalculateNeededCount(); }
#endif

    void RecalculateNeededCount()
    {
        if (!skin)
        {
            _neededCount = Mathf.Clamp(_neededCount, 20, maxStamps);
            return;
        }

        var sz = skin.bounds.size;                 // world units
        float approxArea = Mathf.Max(0.0001f, sz.x * sz.y);

        float step = Mathf.Max(0.01f, minDistanceBetweenStamps);
        // Diện tích tăng thêm mỗi tick ~ bề dày * bước (không dùng L khi step < L)
        float effPerStamp = stampWidth * Mathf.Min(step, stampLength);

        float ff = Mathf.Clamp(fillFactor, 0.1f, 1.0f);
        int n = Mathf.CeilToInt(approxArea / Mathf.Max(0.0001f, effPerStamp * ff));

        _neededCount = Mathf.Clamp(n, 20, maxStamps);
    }

    public void BeginSession()
    {
        if (IsPeeled) return;
        _hasLast = false; // không xoá tem cũ; chỉ reset điểm tham chiếu khoảng cách
      
    }

    public void TrySpawnStamp(Vector2 worldPos, Vector2 dir)
    {
        if (IsPeeled) return;
        if (!PeelStampPool.Instance || !PeelStampPool.Instance.stampPrefab) return;

        if (_hasLast && Vector2.Distance(_lastStampPos, worldPos) < minDistanceBetweenStamps)
            return;

        if (skin && !skin.bounds.Contains(worldPos)) return;
        if (_usedCount >= maxStamps) return;

        // Mượn tem từ pool toàn cục
        var mask = PeelStampPool.Instance.Rent();
        _borrowed.Add(mask);

        // Đặt vị trí/rotation/scale theo world (không parent vào quả)
        mask.transform.position = worldPos;

        float ang = (dir.sqrMagnitude > 0.00004f) ? Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg : 0f;
        mask.transform.rotation = Quaternion.Euler(0, 0, ang);

        var spr = mask.sprite;
        Vector2 sprSize = spr ? (Vector2)spr.bounds.size : new Vector2(1f, 1f);
        float sx = stampLength / Mathf.Max(0.0001f, sprSize.x);
        float sy = stampWidth / Mathf.Max(0.0001f, sprSize.y);
        mask.transform.localScale = new Vector3(sx, sy, 1f);

        _usedCount++;
        _lastStampPos = worldPos;
        _hasLast = true;

        PlayPeelFX(worldPos);
        if (Progress01 >= finishThreshold01)
            CompletePeel();
    }

    public void EndSession() { /* không cần xử lý thêm */ }

    public virtual void CompletePeel()
    {
        if (IsPeeled) return;
        IsPeeled = true;

        if (skin) skin.gameObject.SetActive(false);
        ReturnAllStampsToPool();

        OnPeelFinish();
    }

    public void ResetPeel()
    {
        IsPeeled = false;
        _usedCount = 0;
        _hasLast = false;

        if (skin) skin.enabled = true;
        if (flesh) flesh.enabled = true;
        if (scraps) scraps.SetActive(false);

        ReturnAllStampsToPool();
        RecalculateNeededCount();
    }

    void ReturnAllStampsToPool()
    {
        if (!PeelStampPool.Instance) { _borrowed.Clear(); return; }

        for (int i = 0; i < _borrowed.Count; i++)
        {
            if (_borrowed[i])
                PeelStampPool.Instance.Return(_borrowed[i]);
        }
        _borrowed.Clear();
    }
    public void OnPeelFinish()
    {
        transform.DOKill();           // hủy tween cũ nếu có

        if (!playFinishFX)
        {
            if (scraps) { scraps.SetActive(true); }
            return;
        }

        // Bật scraps ngay trước khi chạy tween, giữ vị trí đã đặt sẵn trong editor
        if (scraps)
        {
            if (!_cachedScrapsScale)
            {
                _scrapsOriginalScale = scraps.transform.localScale;
                _cachedScrapsScale = true;
            }
            scraps.SetActive(true);
            scraps.transform.DOKill();
            Vector3 from = _scrapsOriginalScale * Mathf.Max(0.01f, scrapsPopFrom);
            scraps.transform.localScale = from;
        }

        Vector3 endPos = transform.position;

        var seq = DOTween.Sequence();

        // Nhảy của quả
        seq.Join(transform.DOJump(endPos, jumpPower, Mathf.Max(1, jumpCount), jumpDuration)
                          .SetEase(Ease.OutQuad));

        // Pop scraps chạy song song với nhảy
        if (scraps)
        {
            seq.Join(scraps.transform.DOScale(_scrapsOriginalScale, scrapsPopDuration)
                                     .SetEase(Ease.OutBack));
        }

        seq.Play();
        scraps.transform.SetParent(cuttingBoard, worldPositionStays: true);
    }
    void PlayPeelFX(Vector2 worldPos)
    {
        if (!peelFX) return;
        peelFX.transform.position = worldPos;
        peelFX.Play(true);
    }
}
