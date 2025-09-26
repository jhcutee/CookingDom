    using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("Masks")]
    [SerializeField] private LayerMask pickMask;
    [SerializeField] private LayerMask dropMask;

    public event Action<Collider2D, Vector2> onDragStarted;
    public event Action<Collider2D, Vector2> onDrag;
    public event Action<Collider2D, Vector2> onDragEnded;

    private Camera mainCam;
    private Collider2D currentGrabbed;

    void Awake() { mainCam = Camera.main ?? FindAnyObjectByType<Camera>(); }

    public void SetActiveMask(LayerMask pick, LayerMask drop) { pickMask = pick; dropMask = drop; }
    public void SetPickMask(LayerMask m) => pickMask = m;
    public void SetDropMask(LayerMask m) => dropMask = m;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentGrabbed = PickOne(Input.mousePosition, pickMask);
            if (currentGrabbed)
                onDragStarted?.Invoke(currentGrabbed, ScreenToWorld(Input.mousePosition)); 
        }
        else if (Input.GetMouseButton(0) && currentGrabbed)
        {
            onDrag?.Invoke(currentGrabbed, ScreenToWorld(Input.mousePosition));         
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (currentGrabbed)
                onDragEnded?.Invoke(currentGrabbed, ScreenToWorld(Input.mousePosition));   
            currentGrabbed = null;
        }
    }

    public Collider2D PickOne(Vector2 screenPos, LayerMask mask)
    {
        var w = ScreenToWorld(screenPos);
        return Physics2D.OverlapPoint((Vector2)w, mask); 
    }

    public Collider2D PickDropTarget(Vector2 screenPos)
    {
        var w = ScreenToWorld(screenPos);
        return Physics2D.OverlapPoint((Vector2)w, dropMask);
    }

    public Vector3 ScreenToWorld(Vector2 v)
    {
        float z = Mathf.Abs(mainCam ? mainCam.transform.position.z : 10f);
        return mainCam ? mainCam.ScreenToWorldPoint(new Vector3(v.x, v.y, z)) : new Vector3(v.x, v.y, 0);
    }
}
