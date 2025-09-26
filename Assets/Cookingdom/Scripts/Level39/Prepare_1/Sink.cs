using UnityEngine;

public class Sink : DropTargetBase
{
    protected override bool Accepts(ItemBase item)
    {
        if (item == null) return false;
        return item.HasTag(ItemTag.Washable);
    }
    protected override void OnItemAccepted(ItemBase item)
    {
        base.OnItemAccepted(item);
        if(item != null && SinkController.instance.IsWaterOn)
        {
            item.GetComponent<CleaningBeetroot>()?.Cleaning();
        }
    }        
}
