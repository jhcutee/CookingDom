using UnityEngine;

public class Beef : ItemBroadPlaceableBase
{
    public bool canCut = false;
    public override void OnBoardPlaced()
    {
        ClayPotController.Instance.UnpackPotContents();

        this.transform.localScale = Vector3.one;
    }
}
