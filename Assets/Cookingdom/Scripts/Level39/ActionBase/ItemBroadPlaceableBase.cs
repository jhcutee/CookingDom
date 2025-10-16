using UnityEngine;

public abstract class ItemBroadPlaceableBase : MonoBehaviour
{
    public bool actionDone = false;
    public abstract void OnBoardPlaced();
}
