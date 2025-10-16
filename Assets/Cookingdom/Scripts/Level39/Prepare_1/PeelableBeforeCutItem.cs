using UnityEngine;

public class PeelableBeforeCutItem : PeelableItem
{
    public override void CompletePeel()
    {
        base.CompletePeel();
        if (!flesh)
        {
            return;
        }
        this.gameObject.GetComponent<CuttablePartsItem>().canCut = true;
        flesh.enabled = true;
        flesh.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
    }

}
