using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanController : MonoBehaviour
{
    public static PanController instance;

    [Header("Elements")]
    [SerializeField] private List<GameObject> reciptStep1;
    [SerializeField] private PanTarget panTarget; public PanTarget PanTarget => panTarget;
    [SerializeField] private GasStoveKnob knob; public GasStoveKnob Knob => knob;
    [SerializeField] private PanDragDrop panDragDrop;
    [SerializeField] private WoodenSpoon woodenSpoon;
    [SerializeField] private GameObject soup;
    [SerializeField] private GameObject lid;

    [Tooltip("Parent chứa tất cả item đã thả vào nồi/chảo")]
    [SerializeField] private Transform itemsRoot;

    [Tooltip("Parent chứa tất cả effect (bọt, hơi, vv.)")]
    [SerializeField] private GameObject effectsRoot;

    [Header("Recipe State")]
    [SerializeField] protected int countIngredient = 0;
    [SerializeField] public bool isStep1Complete = false;

    [Header("Stir Input")]
    [SerializeField] float minRadius = 0.25f;
    [SerializeField] float maxRadius = 1.50f;
    [SerializeField] float requiredTurns = 10f;   // 10 vòng => soup alpha = 1
    [SerializeField] bool onlyCCW = true;

    [Header("Sticky Re-grab & Snap")]
    [SerializeField] float minGrabRadius = 0.05f;
    [SerializeField] float ringBias = 0.65f;
    [SerializeField] float minDeltaDeg = 0.25f;
    [SerializeField] float hysteresisIn = 0.85f;
    [SerializeField] float hysteresisOut = 1.10f;

    Camera cam;
    Collider2D panCol;
    SpriteRenderer soupSR;

    bool stirring;
    Vector2 prevVec;
    float accumDeg;

    // ----- orbit cache (quay bằng position) -----
    class Orbit { public float radius; public float angle; public float z; }
    readonly Dictionary<Transform, Orbit> _orbits = new();

    void Awake()
    {
        instance = this;
        cam = Camera.main;
        panCol = GetComponent<Collider2D>();
        if (soup) soupSR = soup.GetComponent<SpriteRenderer>();
        SetSoupAlpha(0f);
    }

    public bool CanDropIntoPan(GameObject ingredient)
    {
        if (isStep1Complete) return true;
        if (countIngredient >= reciptStep1.Count) return false;
        if (ingredient == reciptStep1[countIngredient]) { countIngredient++; return true; }
        return false;
    }

    public bool IsReadyToCook() => !isStep1Complete && countIngredient == reciptStep1.Count && knob && knob.isOn;

    // ===================== INPUT STICKY =====================
    void Update()
    {
        if (isStep1Complete) { stirring = false; return; }
        if (!IsReadyToCook() || !woodenSpoon || !woodenSpoon.isOnPan) { stirring = false; return; }

        bool down = Input.GetMouseButton(0);
        Vector2 mPos = MouseWorldOnPanZ();

        if (down && IsPointerOnPan(mPos))
        {
            Vector2 center = transform.position;
            Vector2 rawVec = mPos - center;
            if (rawVec.magnitude < minGrabRadius) { stirring = false; return; }

            Vector2 snapped = SnapToRing(rawVec);

            if (!stirring)
            {
                stirring = true;
                prevVec = snapped;
                EnsureOrbitCache();
            }
            else
            {
                float delta = Vector2.SignedAngle(prevVec, snapped);
                if (onlyCCW) delta = Mathf.Max(0f, delta);

                if (Mathf.Abs(delta) >= minDeltaDeg)
                {
                    RotateItemsAroundPan(delta);
                    if (woodenSpoon && woodenSpoon.isOnPan) woodenSpoon.ApplyOrbitDelta(delta);
                    accumDeg += delta;
                    UpdateSoupProgress(accumDeg / (360f * requiredTurns));
                }
                prevVec = snapped;
            }
        }
        else stirring = false;
    }

    bool IsPointerOnPan(Vector2 worldPos) => panCol ? panCol.OverlapPoint(worldPos) : true;

    Vector2 SnapToRing(Vector2 v)
    {
        float r = v.magnitude;
        float inMin = minRadius * hysteresisIn;
        float outMax = maxRadius * hysteresisOut;
        r = Mathf.Clamp(r, inMin, outMax);

        float panR = ComputePanInnerRadius();
        float niceR = Mathf.Clamp(panR * ringBias, minRadius, maxRadius);
        float snappedR = Mathf.Lerp(r, niceR, 0.35f);
        return v.normalized * snappedR;
    }

    // ===================== Orbit =====================
    void EnsureOrbitCache()
    {
        if (!itemsRoot) return;

        Vector2 center = transform.position;
        float panR = ComputePanInnerRadius();

        for (int i = 0; i < itemsRoot.childCount; i++)
        {
            var t = itemsRoot.GetChild(i);
            if (!t || !t.gameObject.activeInHierarchy) continue;

            var off = (Vector2)t.position - center;
            float rNow = off.magnitude;
            if (rNow < 0.0001f) rNow = 0.05f;

            float safeR = ComputeSafeItemRadius(t, panR);
            if (rNow > safeR)
            {
                var dir = off.sqrMagnitude > 0f ? off.normalized : Vector2.right;
                var target = (Vector2)center + dir * safeR;
                t.position = new Vector3(target.x, target.y, t.position.z);
                rNow = safeR;
            }

            if (!_orbits.TryGetValue(t, out var o))
                _orbits[t] = new Orbit { radius = Mathf.Clamp(rNow, 0.05f, safeR), angle = Mathf.Atan2(off.y, off.x), z = t.position.z };
            else
            {
                o.radius = Mathf.Clamp(rNow, 0.05f, safeR);
                o.angle = Mathf.Atan2(off.y, off.x);
                o.z = t.position.z;
            }
        }

        var remove = new List<Transform>();
        foreach (var kv in _orbits) if (!kv.Key || kv.Key.parent != itemsRoot) remove.Add(kv.Key);
        foreach (var k in remove) _orbits.Remove(k);
    }

    void RotateItemsAroundPan(float deltaDeg)
    {
        if (!itemsRoot) return;
        EnsureOrbitCache();

        float dRad = deltaDeg * Mathf.Deg2Rad;
        Vector3 center = transform.position;
        float panR = ComputePanInnerRadius();

        foreach (var kv in _orbits)
        {
            var t = kv.Key; var o = kv.Value;
            if (!t || !t.gameObject.activeInHierarchy) continue;

            float safeR = ComputeSafeItemRadius(t, panR);
            o.radius = Mathf.Clamp(o.radius, 0.05f, safeR);

            o.angle += dRad;
            float x = center.x + Mathf.Cos(o.angle) * o.radius;
            float y = center.y + Mathf.Sin(o.angle) * o.radius;
            t.position = new Vector3(x, y, o.z);
        }
    }

    public float GetPanInnerRadius() => ComputePanInnerRadius();

    float ComputePanInnerRadius()
    {
        if (TryGetComponent<CircleCollider2D>(out var cc))
        {
            float scale = Mathf.Abs(transform.lossyScale.x);
            return cc.radius * scale * 0.90f; // trừ vành
        }
        if (TryGetComponent<SpriteRenderer>(out var sr))
        {
            float r = Mathf.Min(sr.bounds.extents.x, sr.bounds.extents.y);
            return r * 0.90f;
        }
        return 1.0f * 0.90f;
    }

    float ComputeSafeItemRadius(Transform item, float panInnerRadius)
    {
        float itemR = ComputeItemRadius(item);
        return Mathf.Max(0.05f, panInnerRadius - itemR - 0.02f);
    }

    float ComputeItemRadius(Transform t)
    {
        var srs = t.GetComponentsInChildren<SpriteRenderer>(true);
        if (srs.Length > 0)
        {
            Bounds b = srs[0].bounds;
            for (int i = 1; i < srs.Length; i++) b.Encapsulate(srs[i].bounds);
            return Mathf.Max(b.extents.x, b.extents.y);
        }
        return 0.05f;
    }

    // ===================== Soup progress & finish =====================
    void UpdateSoupProgress(float k)
    {
        k = Mathf.Clamp01(k);
        SetSoupAlpha(k);

        // Hoàn thành ngay khi alpha = 1
        if (!isStep1Complete && k >= 1f - 1e-4f)
            CompleteStep1();
    }

    void SetSoupAlpha(float a01)
    {
        if (!soup) return;

        if (soupSR)
        {
            var c = soupSR.color; c.a = a01; soupSR.color = c;
            if (!soup.activeSelf && a01 > 0f) soup.SetActive(true);
            return;
        }

        if (soup.TryGetComponent(out CanvasGroup cg))
        {
            cg.alpha = a01;
            if (!soup.activeSelf && a01 > 0f) soup.SetActive(true);
        }
    }

    void CompleteStep1()
    {
        isStep1Complete = true;
        stirring = false;

        // Tắt Effects + Items
        if (effectsRoot) effectsRoot.SetActive(false);
        ToggleItems(false);

        // Trả thìa về chỗ cũ
        if (woodenSpoon) woodenSpoon.ReturnStartPos();
    }

    void ToggleItems(bool on)
    {
        if (!itemsRoot) return;
        for (int i = 0; i < itemsRoot.childCount; i++)
        {
            var t = itemsRoot.GetChild(i);
            if (t) t.gameObject.SetActive(on);
        }
    }

    // util
    Vector2 MouseWorldOnPanZ()
    {
        float z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        return cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));
    }
    public void StartCookingStep2()
    {
        lid.SetActive(true);
        StartCoroutine(TurnOnDrag());
    }
    private IEnumerator TurnOnDrag()
    {
        yield return new WaitForSeconds(5.0f);
        this.GetComponent<Collider2D>().enabled = false;
        knob.TurnOff();
        panDragDrop.canDrag = true;
        panDragDrop.selCollider.enabled = true;
    }
}
