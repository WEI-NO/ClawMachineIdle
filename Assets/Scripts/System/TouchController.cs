using CustomLibrary.References;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles touch interactions for isometric building placement and selection.
/// Manages selection, drag/move, edit mode, and camera controls.
/// </summary>
public class TouchController : MonoBehaviour
{
    // --- Singleton & Instance ---
    public static TouchController Instance { get; private set; }

    // --- Inspector References ---
    [Header("Core References")]
    [SerializeField] private MovingIndicator movingIndicator;

    [Header("Touch Settings")]
    [SerializeField] private float holdThreshold = 1.5f;


    // --- Touch State ---
    [SerializeField] private bool isHolding;
    [SerializeField] private bool isDragging;
    [SerializeField] private bool triggeredHold;
    [SerializeField] private float holdTimer;
    private bool lostHold = false;
    [SerializeField] private int pointerId = -1;
    private Vector2 lastTouchPosition;
    private Vector3 dragOrigin;
    private Vector2Int dragOffset;
    private Vector2Int startGridOrigin;

    // --- Selection State ---
    [SerializeField] private IsometricBuilding targetBuilding;
    public IsometricBuilding selectedBuilding;
    public Action<IsometricBuilding> OnSelectedBuildingChange;

    // --- Edit Mode State ---
    public bool EditMode;

    // --- Unity Events (for UI/Animation hooks) ---
    public UnityEvent OnTouchStart;
    public UnityEvent OnTouchUpdate;
    public UnityEvent OnTouchLeave;
    public UnityEvent OnHoldComplete;

    public Action OnEditModeEnter;
    public Action OnEditModeExit;

    #region Unity Methods

    private void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        Instance = this;
        // Setup or validate any required references here
    }

    private void Update()
    {
        HandleTouchInput();
    }

    #endregion

    #region Touch Processing

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);

        // Early-out for UI
        if (IsPointerOverAnyUI(touch.position)) return;

        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(touch.position);
        IsometricBuilding building = FindBuildingAtScreenPoint(touch.position);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                BeginTouch(touch, worldPoint, building);
                break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                UpdateTouch(touch, worldPoint, building);
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                EndTouch(touch, building);
                break;
        }
    }

    private void BeginTouch(Touch touch, Vector2 worldPoint, IsometricBuilding building)
    {
        // Start hold state
        isHolding = true;
        pointerId = touch.fingerId;
        holdTimer = 0f;
        lastTouchPosition = worldPoint;
        targetBuilding = building;
        isDragging = false;
        triggeredHold = false;

        OnTouchStart?.Invoke();

        // If the finger clicked on a building on 'down'
        if (targetBuilding)
        {
            startGridOrigin = targetBuilding.blueprint.GridPosition;
            dragOrigin = worldPoint;
            if (!EditMode)
            {
                movingIndicator?.Activate(() => holdTimer, () => holdThreshold);
            }
            targetBuilding.PlayAnimation("Held");
            SelectBuilding(targetBuilding, false);
            PlaceableOptions.Instance.SetBuilding(targetBuilding);
            // Set up hold-complete logic only once per touch
            OnHoldComplete.RemoveAllListeners();
            OnHoldComplete.AddListener(() =>
            {
                targetBuilding.PlayAnimation("Selected");
            });
        }
        else
        {
            SelectBuilding(null, false);
            PlaceableOptions.Instance.SetBuilding(null);
            // Start camera drag!
            RoomCamera.Instance.OnDragStart(touch.position);
        }
    }

    private void UpdateTouch(Touch touch, Vector2 worldPoint, IsometricBuilding building)
    {
        if (!isHolding || touch.fingerId != pointerId)
            return;

        holdTimer += Time.deltaTime;
        bool overTarget = building != null && building == targetBuilding;

        // --- Early cancel if finger is off building before hold completes
        if (!overTarget && !triggeredHold && !lostHold)
        {
            // Cancel hold: lose the right to move/drag
            movingIndicator?.Deactivate();
            lostHold = true;

            // Play release animation and deselect if desired
            if (targetBuilding)
            {
                targetBuilding.PlayAnimation("Release");
                SelectBuilding(null, false);
                PlaceableOptions.Instance.SetBuilding(null);
            }
            OnHoldComplete.RemoveAllListeners();
            // DO NOT set isHolding = false or pointerId = -1
            // Allow camera pan for the rest of this touch
            // If the finger left the building, start panning
            RoomCamera.Instance.OnDragStart(touch.position); // Ensure pan drag starts after hold is lost
        }

        if (!isDragging && targetBuilding && !lostHold)
        {
            if (holdTimer >= holdThreshold || EditMode)
            {
                // Trigger hold-complete (enter edit mode)
                triggeredHold = true;
                isDragging = true;
                movingIndicator?.Deactivate();
                OnHoldComplete?.Invoke();
                SetEditMode(true);

                if (targetBuilding)
                    targetBuilding.SetOutline(true, 0.5f);
            }
        }

        // Is dragging and target building exists and either triggered hold or editmode is on and haven't lost hold
        if (isDragging && targetBuilding && (triggeredHold || EditMode) && !lostHold)
        {
            // Dragging logic
            Vector3 offset = worldPoint - (Vector2)dragOrigin;
            offset = new Vector3(32 * offset.x, 32 * offset.y, 0); // TODO: Make 32 a configurable "pixels per grid" field
            dragOffset = startGridOrigin + new Vector2Int(Mathf.RoundToInt(offset.x), Mathf.RoundToInt(offset.y));
            targetBuilding.SetTargetPosition(dragOffset);
        }
        else if (!overTarget)
        {
            // Camera pan (still allowed after lostHold)
            RoomCamera.Instance.OnDragUpdate(touch.position);
        }

        lastTouchPosition = worldPoint;
        OnTouchUpdate?.Invoke();
    }


    private void EndTouch(Touch touch, IsometricBuilding building)
    {
        if (!isHolding || touch.fingerId != pointerId)
            return;

        // End drag/hold state
        isHolding = false;
        lostHold = false;
        pointerId = -1;

        if (targetBuilding)
        {
            // If still holding same building, select it; else, release
            if (targetBuilding == building || triggeredHold)
            {
                targetBuilding.PlayAnimation("Selected");
                SelectBuilding(targetBuilding, false);
                PlaceableOptions.Instance.SetBuilding(targetBuilding);
            }
            else
            {
                targetBuilding.PlayAnimation("Release");
                SelectBuilding(null, false);
                PlaceableOptions.Instance.SetBuilding(null);
            }
            if (isDragging)
            {
                isDragging = false;
                targetBuilding.SetTargetPosition(targetBuilding.blueprint.GridPosition);

            }
        }

        movingIndicator?.Deactivate();
        triggeredHold = false;
        OnHoldComplete.RemoveAllListeners();
        OnTouchLeave?.Invoke();
    }

    #endregion

    #region Building Selection/Helpers

    private void SelectBuilding(IsometricBuilding building, bool isDragging)
    {
        if (selectedBuilding && selectedBuilding != building)
        {
            selectedBuilding.SetSelected(false, isDragging);
        }
        selectedBuilding = building;
        if (selectedBuilding)
        {
            selectedBuilding.SetSelected(true, isDragging);
        }
        OnSelectedBuildingChange?.Invoke(selectedBuilding);
    }

    private IsometricBuilding FindBuildingAtScreenPoint(Vector2 screenPosition)
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);

        IsometricBuilding closest = null;
        float lowestY = float.MaxValue;
        foreach (var hit in hits)
        {
            var building = hit.GetComponentInParent<IsometricBuilding>();
            if (building != null)
            {
                if (building == selectedBuilding)
                {
                    return building;
                }
                float y = building.transform.position.y;
                if (y < lowestY)
                {
                    lowestY = y;
                    closest = building;
                }
            }
        }
        return closest;
    }

    public static bool IsPointerOverAnyUI(Vector2 screenPosition)
    {
        var raycasters = FindObjectsByType<GraphicRaycaster>(sortMode: FindObjectsSortMode.None);
        foreach (var raycaster in raycasters)
        {
            if (!raycaster.enabled || !raycaster.gameObject.activeInHierarchy)
                continue;
            PointerEventData ped = new PointerEventData(EventSystem.current) { position = screenPosition };
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(ped, results);
            if (results.Count > 0) return true;
        }
        return false; 
    }

    #endregion

    #region Edit Mode

    public void SetEditMode(bool enabled)
    {
        if (EditMode == enabled) return;
        EditMode = enabled;
        if (enabled)
        {
            OnEditModeEnter?.Invoke();
        } else
        {
            OnEditModeExit?.Invoke();
        }
        //if (enabled)
        //{
        //    OnSelectedBuildingChange?.Invoke(null); // Deselect in edit mode
        //}
        
        EditModeUI.Instance?.Toggle(EditMode);
    }

    #endregion
}
