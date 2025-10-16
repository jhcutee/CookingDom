using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class CuttablePartsItem : CuttableItemBase
{
    [Header("Cut-on-Flesh Mode")]
    public bool useFleshAsSkin = true;

    [Header("Flesh / Result")]
    public SpriteRenderer fleshRenderer;
    public Sprite resultSprite;
    public SpriteMaskInteraction fleshMaskDuringCut = SpriteMaskInteraction.VisibleOutsideMask;

    [Header("Parts")]
    public Transform partsRoot;
    public bool autoCollectChildren = true;
    public List<Transform> parts = new List<Transform>();

    [Header("Part Bounce")]
    public float partJumpPower = 0.10f;
    public float partJumpDuration = 0.12f;
    public int partJumpCount = 1;
    public Ease partJumpEase = Ease.OutQuad;

    [Header("Flesh Pop On Finish")]
    public bool fleshPopOnFinish = true;
    public float fleshPopScale = 1.2f;
    public float fleshPopUpDuration = 0.12f;
    public Ease fleshPopUpEase = Ease.OutBack;
    public bool fleshReturnToBase = false;
    public float fleshPopDownDuration = 0.10f;
    public Ease fleshPopDownEase = Ease.OutQuad;

    Vector3 fleshBaseScale;

    // --- helper: trả về "skin hiệu lực" cho bước cắt ---
    GameObject EffectiveSkin
        => (useFleshAsSkin && fleshRenderer) ? fleshRenderer.gameObject : skin;

    void Start()
    {
        if (autoCollectChildren && partsRoot && parts.Count == 0)
        {
            parts.Clear();
            for (int i = 0; i < partsRoot.childCount; i++)
                parts.Add(partsRoot.GetChild(i));
        }

        if (fleshRenderer) fleshBaseScale = fleshRenderer.transform.localScale;

        SetAllPartsActive(false);
        if (clickMax <= 0 && parts.Count > 0) clickMax = parts.Count;
    }

    // Bắt đầu cắt: hiển thị “lớp bị cắt” cho dao (mask) — luôn dùng flesh để lộ parts
    public override void ShowResult(bool on)
    {
        if (fleshRenderer)
        {
            fleshRenderer.enabled = true;
            fleshRenderer.maskInteraction = fleshMaskDuringCut;
        }
        SetAllPartsActive(false);
    }

    // Kết thúc cắt
    public override void HideSkin()
    {
        // 1) Ẩn "skin hiệu lực":
        //    - Bắp cải: skin là layer vỏ → tắt 'skin' như thường
        //    - Khoai tây (useFleshAsSkin=true): coi flesh là 'skin' cho bước cắt → tắt flesh GO (nếu muốn)
        //    Trong bài toán của bạn: khoai tây sau cắt sẽ hiện resultSprite trên chính fleshRenderer,
        //    nên ta KHÔNG tắt flesh GO mà đổi sprite + tắt mask.
        var effective = EffectiveSkin;

        // Bắp cải (useFleshAsSkin=false): ẩn 'skin' thật sự
        if (!useFleshAsSkin && effective) effective.SetActive(false);

        // 2) Hoàn thiện hiển thị “kết quả cắt” trên FLESH
        if (fleshRenderer)
        {
            // Dừng dùng mask, chuyển sang sprite kết quả
            fleshRenderer.maskInteraction = SpriteMaskInteraction.None;
            if (resultSprite) fleshRenderer.sprite = resultSprite; Debug.Log(resultSprite.name);
            if (this.gameObject.name == "Carrot")
            {
                fleshRenderer.transform.localRotation = Quaternion.Euler(0,0,-15);
            }
            // Phồng nhẹ cho đẹp
            PopFleshOnce();
        }

        canCut = false;
    }

    void PopFleshOnce()
    {
        if (!fleshRenderer || !fleshPopOnFinish) return;

        var t = fleshRenderer.transform;
        t.DOKill();

        var target = fleshBaseScale * fleshPopScale;

        if (fleshReturnToBase)
        {
            var seq = DOTween.Sequence();
            seq.Append(t.DOScale(target, fleshPopUpDuration).SetEase(fleshPopUpEase));
            seq.Append(t.DOScale(fleshBaseScale, fleshPopDownDuration).SetEase(fleshPopDownEase));
            seq.Play();
        }
        else
        {
            t.localScale = fleshBaseScale;
            t.DOScale(target, fleshPopUpDuration)
             .SetEase(fleshPopUpEase)
             .OnComplete(() => { fleshBaseScale = t.localScale; });
        }
    }

    // KnifeTool gọi để hiện từng khúc theo lượt cắt
    public override void OnCutStep(int clicksDone)
    {
        int idx = clicksDone - 1;
        if (idx < 0 || idx >= parts.Count) return;

        var p = parts[idx];
        if (!p) return;

        p.gameObject.SetActive(true);
        p.DOKill();
        var pos = p.position;
        p.DOJump(pos, partJumpPower, Mathf.Max(1, partJumpCount), partJumpDuration)
         .SetEase(partJumpEase);
    }

    // Sau khi cắt xong: tắt parts (tuỳ flow gameplay của bạn)
    public override void OnCutFinish()
    {
        SetAllPartsActive(false);
        var selfCol = this.GetComponent<Collider2D>();
        selfCol.enabled = false;
        selfCol.enabled = true;
    }

    void SetAllPartsActive(bool on)
    {
        for (int i = 0; i < parts.Count; i++)
            if (parts[i]) parts[i].gameObject.SetActive(on);
    }
}
