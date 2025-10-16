using UnityEngine;
using DG.Tweening;

public class KnifeTool : MonoBehaviour
{
    private static KnifeTool _instance;
    public static KnifeTool Instance => _instance;

    [Header("Drag")]
    [SerializeField] private bool usePickOffset = true;
    [SerializeField] private float returnDuration = 0.25f;
    [SerializeField] private Ease returnEase = Ease.OutQuad;
    public bool canDrag = true;

    [Header("Detect")]
    public string cuttableTag = "Cuttable";
    public Transform cuttingBoard;

    [Header("Visual")]
    public SpriteRenderer sr;
    public Sprite idleSprite;
    public Sprite verticalSprite;
    public GameObject maskGO;
    public Transform knifeVisual;

    [Header("Per-Click Cut (knife motion)")]
    public float strokeY = 0.12f;
    public float strokeDuration = 0.10f;
    public Ease strokeEase = Ease.OutQuad;
    public float advanceX = 0.25f;     // fallback nếu item không set

    Camera cam;
    bool holding;
    Vector3 pickOffset;
    Vector3 restPos;
    Quaternion restRot;
    Tween returnTween;

    public bool cutMode;
    bool stepBusy;
    CuttableItemBase current;
    int clicksDone;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);

        cam = Camera.main;
        Physics2D.queriesHitTriggers = true;

        if (!sr) sr = GetComponentInChildren<SpriteRenderer>(true);
        if (!knifeVisual) knifeVisual = transform;

        SetIdleLook();
    }

    void OnDisable()
    {
        returnTween?.Kill();
        transform.DOKill();
        knifeVisual.DOKill();
    }

    // ---------- Drag ----------
    void OnMouseDown()
    {
        if (cutMode) { DoCutStep(); return; }

        if (!canDrag || holding) return;

        Debug.Log("AAAAA");
        restPos = transform.position;
        restRot = transform.rotation;

        var w = ScreenToWorld(Input.mousePosition);
        pickOffset = usePickOffset ? (transform.position - w) : Vector3.zero;

        holding = true;
        returnTween?.Kill();
    }

    void OnMouseDrag()
    {
        if (!holding || cutMode) return;

        var w = ScreenToWorld(Input.mousePosition);
        var p = w + pickOffset; p.z = transform.position.z;
        transform.position = p;

        var target = FindValidCuttableAt(w);
        if (target && target.knifeAnchor && target.canCut)
            EnterCutMode(target);
    }

    void OnMouseUp()
    {
        if (!holding) return;
        holding = false;
        if (!cutMode) ReturnLastPos();
    }

    void OnMouseExit()
    {
        if (holding && !Input.GetMouseButton(0))
        {
            holding = false;
            if (!cutMode) ReturnLastPos();
        }
    }

    // ---------- Detect ----------
    CuttableItemBase FindValidCuttableAt(Vector3 worldPos)
    {
        var hit = Physics2D.OverlapPoint((Vector2)worldPos);
        var item = TryGetItemIfOnBoard(hit);
        if (item && item.canCut) return item;

        var hits = Physics2D.OverlapPointAll((Vector2)worldPos);
        for (int i = 0; i < hits.Length; i++)
        {
            item = TryGetItemIfOnBoard(hits[i]);
            if (item && item.canCut) return item;
        }
        return null;
    }

    CuttableItemBase TryGetItemIfOnBoard(Collider2D h)
    {
        if (!h || !h.CompareTag(cuttableTag)) return null;
        var c = h.GetComponent<CuttableItemBase>() ?? h.GetComponentInParent<CuttableItemBase>();
        if (!c) return null;

        if (cuttingBoard)
        {
            var drag = c.GetComponent<DraggableItem>();
            if (!(drag && drag.lastSnappedTarget == cuttingBoard)) return null;
        }
        return c;
    }

    // ---------- Cut flow ----------
    void EnterCutMode(CuttableItemBase target)
    {
        holding = false;
        canDrag = false;
        cutMode = true;
        current = target;
        clicksDone = 0;

        current.ShowResult(true);
        if (sr && verticalSprite) sr.sprite = verticalSprite;
        if (maskGO) maskGO.SetActive(true);

        transform.DOKill();
        var wp = current.knifeAnchor.position;
        transform.position = wp;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        cuttingBoard.GetComponent<BroadTarget>().col.enabled = true;


        knifeVisual.DOKill();
    }

    public void DoCutStep()
    {
        if (!cutMode || stepBusy || current == null) return;

        current.PlayCutFX(transform.position);
        stepBusy = true;

        float yDown = -Mathf.Abs(strokeY);
        float y0 = knifeVisual.localPosition.y;

        var seq = DOTween.Sequence();
        seq.Append(knifeVisual.DOLocalMoveY(y0 + yDown, strokeDuration * 0.5f).SetEase(strokeEase));
        seq.Append(knifeVisual.DOLocalMoveY(y0, strokeDuration * 0.5f).SetEase(strokeEase));

        float perFruitStep = (current.knifeAdvanceXPerClick != 0f) ? current.knifeAdvanceXPerClick : advanceX;
        float targetX = transform.position.x + perFruitStep;
        seq.Join(transform.DOMoveX(targetX, strokeDuration).SetEase(DG.Tweening.Ease.OutQuad));

        seq.AppendCallback(() => current.Bounce());
        seq.OnComplete(() =>
        {
            stepBusy = false;
            clicksDone++;
            current.OnCutStep(clicksDone);
            if (clicksDone >= Mathf.Max(1, current.clickMax))
                FinishCut();
        });
        seq.Play();
    }

    void FinishCut()
    {
        if (current) current.HideSkin();
        current?.OnCutFinish();

        cuttingBoard?.GetComponent<BroadTarget>()?.OnItemBoardActionDone();
        cuttingBoard.GetComponent<BroadTarget>().col.enabled = false;


        SetIdleLook();
        cutMode = false;
        canDrag = true;
        current = null;
        
        
        ReturnLastPos();
    }

    // ---------- Visual / Return ----------
    void SetIdleLook()
    {
        if (sr && idleSprite) sr.sprite = idleSprite;
        if (maskGO) maskGO.SetActive(false);
        knifeVisual.DOKill();
    }

    public void ReturnLastPos()
    {
        returnTween?.Kill();
        returnTween = transform
            .DOMove(restPos, returnDuration)
            .SetEase(returnEase)
            .OnComplete(() =>
            {
                transform.position = restPos;
                transform.rotation = restRot;
            });
    }

    Vector3 ScreenToWorld(Vector2 screen)
    {
        float z = Mathf.Abs(cam ? cam.transform.position.z : 10f);
        return cam ? cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, z))
                   : new Vector3(screen.x, screen.y, 0f);
    }
}
