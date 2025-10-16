using UnityEngine;

public class BeetrootRestTarget : DropTargetBase
{
    public override bool CanAccept(DraggableItem item)
    {
        return base.CanAccept(item);
    }
    public override void OnItemDropped(DraggableItem item)
    {
        base.OnItemDropped(item);
    }
}
