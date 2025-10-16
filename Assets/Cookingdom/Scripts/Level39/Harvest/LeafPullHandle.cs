using UnityEngine;
using System.Collections;

public class LeafPullHandle : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Spot spot;
    [SerializeField] private Animator animator;

    [Header("Animator Triggers")]
    [SerializeField] private string clickedTrigger = "Clicked";
    [SerializeField] private string pulledTrigger = "Pulled";
    [SerializeField] private string idleTrigger = "Idle";

    [Header("Tuning")]
    [SerializeField] private float minDragPixels = 6f;
    [SerializeField] private float harvestDelay = 1.5f;
    private Vector2 downScreenPos;
    private bool pressed, pulled;
    private Coroutine harvestCR;

    void Reset()
    {
        spot = GetComponentInParent<Spot>();
        animator = GetComponent<Animator>();
    }

    void OnMouseDown()
    {
        if (spot && spot.IsHarvested) return;

        pressed = true;
        pulled = false;
        downScreenPos = Input.mousePosition;

        StopHarvestCountdown();
        animator?.ResetTrigger(pulledTrigger);
        animator?.SetTrigger(clickedTrigger);
    }

    void OnMouseDrag()
    {
        if (!pressed || pulled) return;

        float dist = ((Vector2)Input.mousePosition - downScreenPos).magnitude;
        if (dist >= minDragPixels)
        {
            pulled = true;
            animator?.ResetTrigger(clickedTrigger);
            animator?.SetTrigger(pulledTrigger);  // kéo đủ -> Pulled
            StartHarvestCountdown();
        }
    }

    void OnMouseUp()
    {
        if (!pressed) return;
        pressed = false;

        // luôn hủy đếm & ép Idle bằng trigger ngay khi thả
        StopHarvestCountdown();
        animator?.ResetTrigger(pulledTrigger);
        animator?.ResetTrigger(clickedTrigger);
        animator?.SetTrigger(idleTrigger);

        pulled = false;
    }

    void StartHarvestCountdown()
    {
        StopHarvestCountdown();
        harvestCR = StartCoroutine(HoldThenHarvest());
    }

    void StopHarvestCountdown()
    {
        if (harvestCR != null)
        {
            StopCoroutine(harvestCR);
            harvestCR = null;
        }
    }

    IEnumerator HoldThenHarvest()
    {
        float t = 0f;
        while (t < harvestDelay)
        {
            if (!pressed) yield break; // thả tay giữa chừng -> không harvest
            t += Time.deltaTime;
            yield return null;
        }

        if (spot && !spot.IsHarvested) spot.Harvest();
        harvestCR = null;
    }

    void OnDisable()
    {
        StopHarvestCountdown();
        pressed = false;
        pulled = false;

        // đảm bảo animator không kẹt ở Pulled nếu object bị disable
        animator?.ResetTrigger(pulledTrigger);
        animator?.SetTrigger(idleTrigger);
    }
}
