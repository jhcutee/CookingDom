using DG.Tweening;
using System.Collections;
using UnityEngine;

public class PepperJar : ItemDraggableToPanBase
{

    public override IEnumerator IEPlayPour()
    {
        animator.SetTrigger("Pour");
        yield return new WaitForSeconds(0.25f);
        this.transform.DOMove(pourAnchor.position - new Vector3(0, 0.2f, 0), 0.2f);
        yield return new WaitForSeconds(0.2f) ;
        this.transform.DOMove(pourAnchor.position, 0.2f);
        yield return new WaitForSeconds(0.55f);
        PourFinished();
        yield return new WaitForSeconds(1f);
        PanController.instance.StartCookingStep2();
    }
}
