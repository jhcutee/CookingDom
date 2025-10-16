using UnityEngine;

public class DrainPlugTarget : DropTargetBase
{
    public override void OnItemDropped(DraggableItem item)
    {
        base.OnItemDropped(item);
        SinkController.instance.OnPlugDroppedTo(transform);
    }
}
