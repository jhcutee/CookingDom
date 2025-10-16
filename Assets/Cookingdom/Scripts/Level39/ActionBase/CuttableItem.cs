using UnityEngine;
using DG.Tweening;

public class CuttableItem : CuttableItemBase
{
    [Header("Result (single GO)")]
    [SerializeField] private GameObject result;

    [Header("Result Pop (per-fruit)")]
    public bool popResultOnFinish = true;
    public float popScaleMultiplier = 1.08f;
    public float popUpDuration = 0.12f;
    public Ease popUpEase = Ease.OutBack;
    public bool returnToOriginal = true;
    public float popDownDuration = 0.10f;
    public Ease popDownEase = Ease.OutQuad;

    public Vector3 resultBaseScale = Vector3.one;

    public override void ShowResult(bool on)
    {
        if (result && result.activeSelf != on)
            result.SetActive(on);
    }

    public override void HideSkin()
    {
        if (skin) skin.SetActive(false);

        if (result)
        {
            if (!result.activeSelf) result.SetActive(true);
            if (popResultOnFinish) PopResultOnce();
        }
        canCut = false;
    }

    protected virtual void PopResultOnce()
    {
        if (!result) return;
        var t = result.transform;
        t.DOKill();

        if (!returnToOriginal)
        {
            t.DOScale(resultBaseScale * popScaleMultiplier, popUpDuration).SetEase(popUpEase);
            return;
        }

        var seq = DOTween.Sequence();
        seq.Append(t.DOScale(resultBaseScale * popScaleMultiplier, popUpDuration).SetEase(popUpEase));
        seq.Append(t.DOScale(resultBaseScale, popDownDuration).SetEase(popDownEase));
        seq.Play();
    }
}
