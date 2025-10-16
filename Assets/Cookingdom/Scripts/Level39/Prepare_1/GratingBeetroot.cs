using DG.Tweening;
using UnityEngine;

public class GratingBeetroot : MonoBehaviour
{
    public bool canGrate = true;

    // Detect grater
    public string graterTag = "Grater";
    public float minMoveWorld = 0.03f;

    // Bào nhiều lần
    public int passesNeeded = 5;            // số lần bào để hoàn tất
    public float perPassDuration = 0.12f;   // thời gian thu nhỏ mỗi lần
    public Ease perPassEase = Ease.OutQuad;

    // Thu nhỏ (nếu muốn khác với pass cuối cùng)
    public float shrinkDuration = 0.6f;
    public Ease shrinkEase = Ease.InQuad;

    public bool disableDraggingOnGrate = true;

    // Rung phần mặt bào mỗi lần bào
    public Transform graterShredPart;
    public float shredShakeDuration = 0.12f;
    public Vector3 shredShakeStrength = new Vector3(0.03f, 0.03f, 0f);
    public int shredShakeVibrato = 10;
    public float shredShakeRandomness = 90f;

    // Bay phần mặt bào ra ngoài theo hình cung (không cần target object)
    public Vector3 shredFlyTargetWorld = new Vector3(7f, 5f, 0f);
    public float shredFlyJumpPower = 1.0f;
    public int shredFlyJumpCount = 1;
    public float shredFlyDuration = 0.6f;
    public Ease shredFlyEase = Ease.OutQuad;

    // Kết quả bào xong (đặt sẵn ở đế, inactive)
    public GameObject gratedResult;

    // Internal
    Camera _cam;
    Vector3 _prevWorld;
    bool _mouseHolding;
    bool _isFinishing;          // đang chạy sequence hoàn tất
    bool _isDone;               // đã xong
    bool _passTweening;         // đang tween 1 lần bào
    int _passesDone;
    Vector3 _startScale;
    Sequence _finishSeq;

    void Awake()
    {
        _cam = Camera.main;
        Physics2D.queriesHitTriggers = true;
        _startScale = transform.localScale;
    }

    void OnDisable() { _finishSeq?.Kill(); }
    void OnDestroy() { _finishSeq?.Kill(); }

    void OnMouseDown()
    {
        if (!canGrate || _isDone || _isFinishing) return;
        _mouseHolding = true;
        _prevWorld = ScreenToWorld(Input.mousePosition);
    }

    void OnMouseUp() { _mouseHolding = false; }

    void OnMouseDrag()
    {
        if (!canGrate || _isDone || _isFinishing) return;
        if (!_mouseHolding) return;

        var curr = ScreenToWorld(Input.mousePosition);
        if (Vector2.Distance(_prevWorld, curr) < minMoveWorld)
        {
            _prevWorld = curr;
            return;
        }

        bool hitPrev = HitGraterAt(_prevWorld);
        bool hitCurr = HitGraterAt(curr);

        // Ít nhất 1 điểm chạm vào grater
        if ((hitPrev || hitCurr) && !_passTweening)
        {
            DoOneGratePass();       // bào một lần
        }

        _prevWorld = curr;
    }

    // ---- một lần bào: lắc mặt bào + giảm scale một nấc ----
    void DoOneGratePass()
    {
        if (_isDone || _isFinishing) return;

        // Tắt kéo ngay từ lần đầu nếu muốn
        if (disableDraggingOnGrate && _passesDone == 0)
        {
            var drag = GetComponent<DraggableItem>();
            if (drag) drag.draggable = false;
        }

        ShakeShredPartOnce();

        _passesDone = Mathf.Min(_passesDone + 1, passesNeeded);
        float t = 1f - (float)_passesDone / Mathf.Max(1, passesNeeded);
        Vector3 targetScale = _startScale * Mathf.Max(0f, t);

        _passTweening = true;
        transform.DOScale(targetScale, perPassDuration).SetEase(perPassEase)
                 .OnComplete(() =>
                 {
                     _passTweening = false;

                     // Đủ số lần thì chạy finish
                     if (_passesDone >= passesNeeded && !_isFinishing)
                         StartFinishSequence();
                 });
    }

    // ---- finish: thu nhỏ về 0 -> bay mặt bào -> bật thành phẩm -> ẩn củ ----
    void StartFinishSequence()
    {
        if (_isFinishing || _isDone) return;
        _isFinishing = true;

        _finishSeq?.Kill();
        _finishSeq = DOTween.Sequence();

        // Thu nhỏ về 0 (nếu còn > 0)
        if (transform.localScale.sqrMagnitude > 0.0001f)
            _finishSeq.Append(transform.DOScale(Vector3.zero, shrinkDuration).SetEase(shrinkEase));
        else
            _finishSeq.AppendInterval(0.01f);

        // KHÔNG SetActive(false) ở đây, để sequence được chạy tiếp

        // Bay phần mặt bào
        if (graterShredPart)
        {
            graterShredPart.SetParent(null, true);
            _finishSeq.Append(
                graterShredPart
                    .DOJump(shredFlyTargetWorld, shredFlyJumpPower, Mathf.Max(1, shredFlyJumpCount), shredFlyDuration)
                    .SetEase(shredFlyEase)
            );
        }
        else
        {
            _finishSeq.AppendInterval(0.1f);
        }
        gratedResult.SetActive(true);
        gratedResult.GetComponent<BoxCollider2D>().enabled = false;
        gratedResult.GetComponent<BoxCollider2D>().enabled = true;

        // Cuối cùng mới ẩn củ (để không Kill sequence sớm)
        _finishSeq.AppendCallback(() =>
        {
            gameObject.GetComponent<Collider2D>().enabled = false;
            gameObject.SetActive(false);
        });

        _finishSeq.OnComplete(() => { _isFinishing = false; });
        _finishSeq.Play();
    }

    // Rung phần mặt bào mỗi lần bào (ngắn)
    void ShakeShredPartOnce()
    {
        if (!graterShredPart) return;
        graterShredPart.DOKill();
        graterShredPart
            .DOShakePosition(shredShakeDuration, shredShakeStrength, shredShakeVibrato, shredShakeRandomness, false, true)
            .SetEase(Ease.Linear);
    }

    // ---- helpers ----
    bool HitGraterAt(Vector2 world)
    {
        // quét tất cả collider tại điểm rồi lọc theo tag
        var hits = Physics2D.OverlapPointAll(world);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (!h) continue;
            if (h.CompareTag(graterTag)) return true;
        }
        return false;
    }

    Vector3 ScreenToWorld(Vector2 s)
    {
        float z = Mathf.Abs(_cam ? _cam.transform.position.z : 10f);
        return _cam ? _cam.ScreenToWorldPoint(new Vector3(s.x, s.y, z))
                    : new Vector3(s.x, s.y, 0f);
    }
}
