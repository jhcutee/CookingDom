using UnityEngine;
using DG.Tweening;

public abstract class CuttableItemBase : MonoBehaviour
{
    [Header("Common Elements")]
    [SerializeField] protected GameObject skin;   // lớp vỏ để ẩn sau khi cắt
    public Transform knifeAnchor;                 // điểm đặt dao khi cắt
    public ParticleSystem cutFX;                  // FX mỗi nhát cắt
    public bool canCut = true;                    // có cho cắt không

    [Header("Cut Settings (shared)")]
    public int clickMax = 5;                    // số nhát cắt cần
    public bool requireOnBoard = true;
    public bool isOnBoard = true;
    public float knifeAdvanceXPerClick = 0.5f;    // dao dịch theo X mỗi click

    [Header("Bounce (shared)")]
    public bool bounceOnEachCut = true;
    public float bounceJumpPower = 0.15f;
    public float bounceDuration = 0.12f;
    public int bounceCount = 1;

    public virtual void ShowResult(bool on) { }   // bật phần “kết quả” để mask lộ dần
    public virtual void HideSkin()                // kết thúc: ẩn skin, khoá cắt
    {
        if (skin) skin.SetActive(false);
        canCut = false;
    }

    public void PlayCutFX(Vector3 worldPos)
    {
        if (!cutFX) return;
        cutFX.transform.position = worldPos;
        cutFX.Play(true);
    }

    public void Bounce()
    {
        if (!bounceOnEachCut) return;
        var endPos = transform.position;
        transform.DOKill();
        transform.DOJump(endPos, bounceJumpPower, Mathf.Max(1, bounceCount), bounceDuration)
                 .SetEase(Ease.OutQuad);
    }

    // KnifeTool sẽ gọi các hook này (mặc định không làm gì)
    public virtual void OnCutStep(int clicksDone) { }
    public virtual void OnCutFinish() { }

    // Cho phép click trực tiếp lên quả để “cắt tiếp” khi dao đang ở chế độ cắt
    void OnMouseDown()
    {
        if (KnifeTool.Instance != null && KnifeTool.Instance.cutMode)
            KnifeTool.Instance.DoCutStep();
    }
}
