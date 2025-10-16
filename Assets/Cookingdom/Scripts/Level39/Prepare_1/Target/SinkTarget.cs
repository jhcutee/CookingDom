using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class SinkTarget : DropTargetBase
{
    public bool isOccupied { get; private set; } = false;
    public override void OnItemDropped(DraggableItem item)
    {
        base.OnItemDropped(item);
        if(SinkController.instance != null)
        {
            WashItemInSink(item);
            isOccupied = true;
        }
        if(item.name == "ClayPot")
        {
            item.GetComponent<ClayPot>().OnDropSink();
        }
    }   
    public void WashItemInSink(DraggableItem item)
    {
        if (SinkController.instance.SinkWater.gameObject.activeInHierarchy)
        {
            var beetrootItem = item.GetComponent<BeetrootItem>();
            beetrootItem.WashBeetroot();
        }
    }
}
