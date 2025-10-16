using UnityEngine;

public class PotatoItem : ItemBroadPlaceableBase
{
    private void Awake()
    {
        GetComponent<CuttableItemBase>().canCut = false;
    }
    public override void OnBoardPlaced()
    {

    }
}
