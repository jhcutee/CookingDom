using UnityEngine;

public class GasStoveTarget : DropTargetBase
{
    public override void OnItemDropped(DraggableItem item)
    {
        item.GetComponent<ClayPot>().OnDropGasStove();
    }
}
