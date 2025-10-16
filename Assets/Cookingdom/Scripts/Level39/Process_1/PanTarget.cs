using DG.Tweening;
using UnityEngine;

public class PanTarget : DropTargetBase
{
    [Header("ItemAnchor")]
    [SerializeField] private Transform rootAnchors;
    [SerializeField] private Transform rootItems;

    [Header("Settings")]
    [SerializeField, Range(0.1f, 1f)] private float dropScaleRatio = 0.8f;
    [SerializeField] private float scaleTweenTime = 0.15f;
    [SerializeField] private Ease scaleEase = Ease.OutQuad;
    public override bool CanAccept(DraggableItem item)
    {
        return PanController.instance.CanDropIntoPan(item.gameObject);
    }
    public override void OnItemDropped(DraggableItem item)
    {
        base.OnItemDropped(item);
        if (!item) return;
        item.draggable = false;
        var t = item.transform;
        ApplyDropScale(t);
        SetParentUnderRootItems(t);
        SnapToAnchorIfAny(t); // chỉ set pos ngay nếu có anchor
    }

    public bool TryGetAnchor(string itemName, out Transform anchor)
    {
        anchor = FindAnchorFor(itemName);
        return anchor != null;
    }

    void ApplyDropScale(Transform t)
    {
        var targetScale = t.localScale * dropScaleRatio;
        // KHÔNG Kill(target) để khỏi phá tween di chuyển của DraggableItem
        t.DOScale(targetScale, scaleTweenTime)
         .SetEase(scaleEase)
         .SetLink(t.gameObject); // auto-kill khi object bị destroy
    }


    public void SetParentUnderRootItems(Transform t)
    {
        if (rootItems) t.SetParent(rootItems, true);
    }

    public void SnapToAnchorIfAny(Transform t)
    {
        if (!rootAnchors) return;
        if (FindAnchorFor(t.name) is Transform a) t.position = a.position;
    }

    Transform FindAnchorFor(string itemName)
    {
        if (!rootAnchors) return null;
        string key = Sanitize(itemName);
        for (int i = 0; i < rootAnchors.childCount; i++)
        {
            var c = rootAnchors.GetChild(i);
            if (Sanitize(c.name) == key) return c;
        }
        return null;
    }

    string Sanitize(string s) => string.IsNullOrEmpty(s) ? s : s.Replace("(Clone)", "").Trim();
}
