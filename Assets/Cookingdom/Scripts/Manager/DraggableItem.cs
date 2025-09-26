using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableItem : MonoBehaviour
{
    [Header("Motion")]
    public bool keepZ = true;
    public float revertDuration = 0.12f;

    [Header("Elements")]
    private InputManager inputManager;
    private Collider2D selfCol;
    

    [Header("Settings")]
    private Vector3 startPos;
    private float z;
    private bool dragging;
    void Awake()
    {
        inputManager = FindAnyObjectByType<InputManager>();
        selfCol = GetComponent<Collider2D>();
    }
    private void OnEnable()
    {
        if (inputManager == null) return;
        inputManager.onDragStarted += OnDragStarted;
        inputManager.onDrag += OnDrag;
        inputManager.onDragEnded += OnDragEnded;
    }
    private void OnDisable()
    {
        if (inputManager == null) return;
        inputManager.onDragStarted -= OnDragStarted;
        inputManager.onDrag -= OnDrag;
        inputManager.onDragEnded -= OnDragEnded;
    }
    private void OnDragStarted(Collider2D hit, Vector2 world)
    {
        if (hit != selfCol) return;

        //undock current target
        var currentTarget = GetComponent<DockedToTarget>();
        if (currentTarget != null)
        {
            currentTarget.target?.Undock(this);
            Destroy(currentTarget);
        }

        dragging = true;
        startPos = transform.position; 
        z = transform.position.z;
    }
    private void OnDrag(Collider2D hit, Vector2 world)
    {
        if (!dragging || hit != selfCol) return;
        transform.position = new Vector3(world.x, world.y, keepZ ? z : transform.position.z);
    }
    private void OnDragEnded(Collider2D hit, Vector2 world)
    {
        if (!dragging || hit != selfCol) return;
        dragging = false;

        var dropCol = inputManager.PickDropTarget(Input.mousePosition);
        var target = dropCol ? dropCol.GetComponent<IDropTarget>() : null;
        if (target != null && target.CanAccept(this, out var snapPos))
        {
            transform.position = snapPos;
            target.Accept(this);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(Revert(startPos, revertDuration));
        }
    }
    System.Collections.IEnumerator Revert(Vector3 back, float dur) 
    { 
        Vector3 a = transform.position; 
        float t = 0f; 
        while (t < 1f) 
        { 
            t += Time.deltaTime / Mathf.Max(0.0001f, dur); 
            transform.position = Vector3.Lerp(a, back, t); 
            yield return null; 
        } 
        transform.position = back; 
    }
}
