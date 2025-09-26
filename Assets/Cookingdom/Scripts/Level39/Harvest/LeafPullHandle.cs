using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class LeafPullHandle : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Spot spot;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private string pullTrigger = "Pulled";
    [SerializeField] private string idleTrigger = "Idle";
    [SerializeField] private float timing = 1.25f;

    private Collider2D selfCol;
    private bool dragging;
    private bool harvestedStarted;
    private float startTime;


    private void Reset()
    {
        inputManager = FindAnyObjectByType<InputManager>();
        spot = GetComponentInParent<Spot>();
        animator = GetComponent<Animator>();
    }
    void Awake()
    {
        selfCol = GetComponent<Collider2D>();

        if (!inputManager) inputManager = FindAnyObjectByType<InputManager>();
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

        if (hit != selfCol || spot == null || spot.IsHarvested) return;

        dragging = true;
        harvestedStarted = false;
        startTime = Time.time;
        animator.SetTrigger(pullTrigger);
    }
    private void OnDrag(Collider2D hit, Vector2 world)
    {
        if (!dragging || hit != selfCol || harvestedStarted) return;

        if(Time.time - startTime >= 0.5f)
        {
            harvestedStarted = true;
            spot.Harvest();
        }

        if (Time.time - startTime >= timing)
        {
            animator?.SetTrigger(idleTrigger);
        }
    }
    private void OnDragEnded(Collider2D hit, Vector2 world)
    {
        if (hit != selfCol) return;
        if(Time.time - startTime < timing && spot != null && spot.IsHarvested)
        {
            spot.StopHarvest();
        }
        animator?.SetTrigger(idleTrigger);

        dragging = false;
    }
}