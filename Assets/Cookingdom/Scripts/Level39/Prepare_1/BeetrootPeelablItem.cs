using UnityEngine;

public class BeetrootPeelablItem : PeelableItem
{
    public override void CompletePeel()
    {
        base.CompletePeel();
        BroadTarget.instance.OnItemBoardActionDone();
        this.GetComponent<GratingBeetroot>().canGrate = true;
    }
}
