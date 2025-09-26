using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class DropTargetBase : MonoBehaviour, IDropTarget
{
    [Header("Snap")]
    public Transform snapPoint;
    public bool keepItemZ;
    public bool makeChildOfSnap;

    [Header("Capacity")]
    public int capacity = 1;

    [Header("HolderItem")]
    protected readonly List<DraggableItem> occupants = new();
    public bool isOccupied => occupants.Count >= capacity;


    protected abstract bool Accepts(ItemBase item);
    protected virtual void OnItemAccepted(ItemBase item) { }
    protected virtual void OnItemRemoved(ItemBase item) { }

    public bool CanAccept(DraggableItem draggable, out Vector3 snapPosition)
    {
        snapPosition = this.transform.position;
        if(draggable == null || isOccupied) return false;

        var item = draggable.GetComponent<ItemBase>();
        if(!item || !Accepts(item)) return false;

        if (keepItemZ)
        {
            var p = snapPosition;
            p.z = draggable.transform.position.z;
            snapPosition = p;
        }
        return true;
    }

    public void Accept(DraggableItem draggable)
    {
        if (!CanAccept(draggable, out Vector3 snapPosition)) return;

        // Snap to position
        draggable.transform.position = snapPosition;


        //set parent
        var slot =  snapPoint ? snapPoint : this.transform;
        if (makeChildOfSnap) draggable.transform.SetParent(slot, worldPositionStays: true);

        occupants.Add(draggable);

        var dock = draggable.GetComponent<DockedToTarget>() ?? draggable.gameObject.AddComponent<DockedToTarget>();
        dock.target = this;

        var item = draggable.GetComponent<ItemBase>();
        item?.OnDocked(this);
        OnItemAccepted(item);
    }
    public void Undock(DraggableItem draggable)
    {
        if (draggable == null) return;

        if(occupants.Remove(draggable))
        {
            if(makeChildOfSnap)
                if(draggable.transform.IsChildOf(this.transform))
                    draggable.transform.SetParent(null, worldPositionStays: true);
        }
        var item = draggable.GetComponent<ItemBase>();
        item?.OnUndocked(this);
        OnItemRemoved(item);
    }
}
public class DockedToTarget : MonoBehaviour
{
    public DropTargetBase target;
}