using UnityEngine;

public class PlugHolder : DropTargetBase
{

    protected override bool Accepts(ItemBase item)
    {
        if (item == null) return false;
        return item.HasTag(ItemTag.Plug);
    }
}
