using UnityEngine;

public interface IDropTarget
{
    bool CanAccept(DraggableItem item, out Vector3 snapPosition);
        
    void Accept(DraggableItem item);
}
