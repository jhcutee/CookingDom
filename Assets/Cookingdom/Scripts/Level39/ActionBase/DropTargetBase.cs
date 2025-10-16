using UnityEngine;

public abstract class DropTargetBase : MonoBehaviour
{
    [Header("Settings")]
    public Vector3 snapOffsetLocal = Vector3.zero;
    public bool offsetInTargetSpace = true;
    public bool applyRotationZ = true;
    public float rotationZ = 0f;

    public virtual bool CanAccept(DraggableItem item) => true;
    public virtual void OnItemDropped(DraggableItem item) { }

    public virtual Vector3 GetSnapWorldPosition(DraggableItem item)
    {
        Vector3 p = offsetInTargetSpace ? transform.TransformPoint(snapOffsetLocal)
                                        : transform.position + snapOffsetLocal;
        if (item && item.keepZ) p.z = item.transform.position.z;
        return p;
    }
    public bool TryGetRotationZ(out float z)
    {
        if (applyRotationZ) { z = rotationZ; return true; }
        z = 0f; return false;
    }
}
