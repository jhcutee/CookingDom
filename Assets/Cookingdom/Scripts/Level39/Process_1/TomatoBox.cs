using DG.Tweening;
using UnityEngine;

public class TomatoBox : MonoBehaviour
{
    [SerializeField] private Sprite unbox;
    [SerializeField] private SpriteRenderer selSP;
    private bool isUnbox = false;
    [Header("Shake (Rotation Z)")]
    [SerializeField] float duration = 0.18f;
    [SerializeField] float strengthDeg = 10f;  // biên độ rung (độ)
    [SerializeField] int vibrato = 12;
    [SerializeField] float randomness = 90f;
    [SerializeField] bool fadeOut = true;
    [SerializeField] bool resetRotationAfter = true;

    Quaternion baseRot;

    private void Awake()
    {
        selSP = GetComponent<SpriteRenderer>();
        baseRot = transform.localRotation;
    }
    private void OnMouseDown()
    {
        if (isUnbox) return;
        selSP.sprite = unbox;
        ShakeTheBox();
        isUnbox = true;
    }
    public void ShakeTheBox()
    {
        transform.DOKill();                          // tránh chồng tween
        transform.localRotation = baseRot;           // về góc gốc trước khi rung
        transform
            .DOShakeRotation(duration, new Vector3(0f, 0f, strengthDeg), vibrato, randomness, fadeOut)
            .SetTarget(transform)
            .SetLink(gameObject)
            .OnComplete(() => { if (resetRotationAfter) transform.localRotation = baseRot; });
    }
}
