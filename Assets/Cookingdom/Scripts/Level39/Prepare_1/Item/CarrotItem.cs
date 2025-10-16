using UnityEngine;

public class CarrotItem : ItemBroadPlaceableBase
{
    
    public override void OnBoardPlaced()
    {
        this.GetComponent<Animator>().SetTrigger("Placed");
    }
}
