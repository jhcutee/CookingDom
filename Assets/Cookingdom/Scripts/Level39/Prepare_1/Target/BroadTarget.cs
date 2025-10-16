using UnityEngine;

public class BroadTarget : DropTargetBase
{
    public static BroadTarget instance;
    public ItemBroadPlaceableBase currentItem { get; private set; }
    public Collider2D col;
    private void Awake()
    {
        instance = this;   
        col = GetComponent<Collider2D>();
        col.enabled = false;
    }
    public override bool CanAccept(DraggableItem item)
    {
        var beetrootItem = item.GetComponent<BeetrootItem>();
        if(beetrootItem != null)
        {
            return beetrootItem.isWashed;
        }
        if(item.GetComponent<ItemBroadPlaceableBase>().actionDone == true)
        {
            return false;
        }
        if(item.name == "Beef")
        {
            return item.GetComponent<Beef>().canCut;
        }
        return currentItem == null;
    }
    public override void OnItemDropped(DraggableItem item)
    {
        base.OnItemDropped(item);
        var itemBoardPlaceable = item.GetComponent<ItemBroadPlaceableBase>();
        itemBoardPlaceable.OnBoardPlaced();
        currentItem = itemBoardPlaceable;
        item.draggable = itemBoardPlaceable.actionDone;
    }
    public void OnItemBoardActionDone()
    {
        currentItem.GetComponent<DraggableItem>().draggable = true;
        currentItem.actionDone = true;
        if(currentItem.name == "Chicken")
        {
            currentItem.GetComponent<Chicken>().canCook = true;
        }
    }
    public void OnItemPickedUpFromBoard()
    {
        currentItem = null;
    }
    private void OnMouseDown()
    {
        if(KnifeTool.Instance.cutMode)
        {
            KnifeTool.Instance.DoCutStep();
        }
    }
}
