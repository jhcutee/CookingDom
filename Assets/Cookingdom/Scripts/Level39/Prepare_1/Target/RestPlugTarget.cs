using UnityEngine;

public class RestPlugTarget : DropTargetBase
{
    public override void OnItemDropped(DraggableItem item)
    {
        base.OnItemDropped(item);
        SinkController.instance.OnPlugDroppedTo(this.transform);
    }
}
