using UnityEngine;
using System;

public class PlugSocket : DropTargetBase
{
    public bool Plugged => occupants.Count > 0;
    public event Action<bool> OnPlugged;
    protected override bool Accepts(ItemBase item)
    {
        if (item == null) return false;
        return item.HasTag(ItemTag.Plug);
    }
    protected override void OnItemAccepted(ItemBase item)
    {
        base.OnItemAccepted(item);
        OnPlugged?.Invoke(Plugged);
    }
    protected override void OnItemRemoved(ItemBase item)
    {
        base.OnItemRemoved(item);
        OnPlugged?.Invoke(Plugged);
    }
}
