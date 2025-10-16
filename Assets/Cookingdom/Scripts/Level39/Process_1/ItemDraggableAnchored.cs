using UnityEngine;

public class ItemDraggableAnchored : DraggableItem
{
    protected override Vector3 ResolveSnapDestination(Transform best, DropTargetBase tBase)
    {
        // Nếu target là Pan và có anchor trùng tên item -> snap vào anchor
        if (tBase is PanTarget pan && pan.TryGetAnchor(transform.name, out var anchor))
        {
            var dst = anchor.position;
            if (keepZ) dst.z = startZ;
            return dst;
        }

        // ngược lại dùng cách mặc định
        return base.ResolveSnapDestination(best, tBase);
    }
}
