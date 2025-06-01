using CustomLibrary.References;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchController : MonoBehaviour
{
    [SerializeField] private IsometricBuilding targetBuilding;
    public IsometricBuilding selectedBuilding;

    public Action<IsometricBuilding> OnSelectedBuildingChange;

    public static TouchController Instance;

    private GraphicRaycaster _raycaster;

    // Adjustable hold time threshold (seconds)
    public float holdThreshold = 1.5f;

    private bool isHolding = false;
    private bool isDragging = false;
    private float holdTimer = 0f;
    private Vector2Int dragOffset;
    private Vector3 dragOrigin;
    private Vector2Int startOrigin;

    public UnityEvent OnTouchStart;
    public UnityEvent OnTouchUpdate;
    public UnityEvent OnTouchLeave;
    public UnityEvent OnHoldComplete;

    public MovingIndicator movingIndicator;
    private bool triggeredHold = false;

    [Header("Camera Controls")]
    public float CameraPanSensitivity = 1.0f;

    [Header("Building Size Visual")]

    // Touch or mouse id
    private int pointerId = -1;

    void Awake()
    {
        Initializer.SetInstance(this);
    }

    // Update is called once per frame
    void Update()
    {
        HandleTouch();
    }

    private Vector2 lastTouchPosition; // Add this to your class if not already

    void HandleTouch()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        Vector2 wp = Camera.main.ScreenToWorldPoint(touch.position);

        if (IsPointerOverAnyUI(touch.position))
        {
            return;
        }

        IsometricBuilding buildingUnderFinger = FindBuildingOnTouch();

        switch (touch.phase)
        {
            case TouchPhase.Began:
                HandleTouchBegan(touch, wp, buildingUnderFinger);
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                HandleTouchDrag(touch, wp, buildingUnderFinger);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                HandleTouchEnd(touch, buildingUnderFinger);
                break;
        }
    }

    private void HandleTouchBegan(Touch touch, Vector2 wp, IsometricBuilding buildingUnderFinger)
    {
        lastTouchPosition = wp;
        isHolding = true;
        holdTimer = 0f;
        pointerId = touch.fingerId;
        targetBuilding = null;

        if (buildingUnderFinger)
        {
            targetBuilding = buildingUnderFinger;
            dragOrigin = (Vector3)wp;
            startOrigin = targetBuilding.blueprint.GridPosition;

            // Visuals and selection
            movingIndicator.Activate(() => holdTimer, () => holdThreshold);
            targetBuilding.PlayAnimation("Held");
            SelectBuilding(targetBuilding, false);

            // Register callback for hold complete
            OnHoldComplete.AddListener(() => { targetBuilding.PlayAnimation("Selected"); });
        }
    }

    private void HandleTouchDrag(Touch touch, Vector2 wp, IsometricBuilding buildingUnderFinger)
    {
        if (!isHolding || touch.fingerId != pointerId) return;

        bool isOffBuilding = (buildingUnderFinger == null || buildingUnderFinger != targetBuilding || targetBuilding == null) && !triggeredHold;
        if (!isDragging)
        {
            isDragging = true;
            if (targetBuilding)
                startOrigin = targetBuilding.blueprint.GridPosition;
        }

        if (isOffBuilding)
        {
            HandleDragOffBuilding(wp);
            return;
        }

        // Continue normal hold/drag logic if still on the same building
        Vector3 offset = (Vector3)wp - dragOrigin;
        offset = new Vector3(32 * offset.x, 32 * offset.y, 0);
        dragOffset = startOrigin + new Vector2Int(Mathf.RoundToInt(offset.x), Mathf.RoundToInt(offset.y));

        holdTimer += Time.deltaTime;

        if (holdTimer >= holdThreshold && !triggeredHold)
        {
            movingIndicator.Deactivate();
            OnHoldComplete?.Invoke();
            triggeredHold = true;
            if (targetBuilding)
                targetBuilding.SetOutline(true, 0.5f);
        }

        if (isDragging && triggeredHold)
        {
            targetBuilding.SetTargetPosition(dragOffset);
        }

        lastTouchPosition = wp;
    }

    private void HandleDragOffBuilding(Vector2 wp)
    {
        SelectBuilding(null, false);

        if (isDragging)
        {
            Vector2 delta = wp - lastTouchPosition;
            delta *= CameraPanSensitivity;
            // Invert if you want "drag world, not camera" feel
            RoomCamera.Instance.SetTargetPosition(Camera.main.transform.position - (Vector3)delta);
            lastTouchPosition = wp;
        }

        // If finger has left original building before drag starts, cancel hold
        if (holdTimer < holdThreshold)
        {
            movingIndicator.Deactivate();
            OnHoldComplete.RemoveAllListeners();
            triggeredHold = false;
            holdTimer = 0f;
            if (targetBuilding)
                targetBuilding.PlayAnimation("Release");
        }
    }

    private void HandleTouchEnd(Touch touch, IsometricBuilding buildingUnderFinger)
    {
        if (!isHolding || touch.fingerId != pointerId) return;

        isHolding = false;
        pointerId = -1;

        if (targetBuilding)
        {
            // If touch is let go on the same building selected, play the select animation and outline
            if (targetBuilding == buildingUnderFinger || triggeredHold)
            {
                targetBuilding.PlayAnimation("Selected");
                SelectBuilding(targetBuilding, false);
            }
            // If touch is let go and hovering over a different building, AND hold hasn't been triggered on the targetBuilding
            else
            {
                targetBuilding.PlayAnimation("Release");
                SelectBuilding(null, false);
            }
        }

        if (isDragging && targetBuilding)
        {
            isDragging = false;
            targetBuilding.SetTargetPosition(targetBuilding.blueprint.GridPosition);
        }
        movingIndicator.Deactivate();
        triggeredHold = false;
        OnHoldComplete.RemoveAllListeners();
    }


    private void SelectBuilding(IsometricBuilding building, bool isDragging)
    {
        var lastSelected = selectedBuilding;
        // If selecting another building while a different building is selected.
        if (selectedBuilding && selectedBuilding != building)
        {
            selectedBuilding.SetSelected(false, isDragging);
            selectedBuilding = null;
        }

        // Select the building.
        if (building)
        {
            selectedBuilding = building;
            selectedBuilding.SetSelected(true, isDragging);
        }

        if (lastSelected != selectedBuilding)
        {
            OnSelectedBuildingChange?.Invoke(selectedBuilding);
        }
    }

    private IsometricBuilding FindBuildingOnTouch()
    {
        Touch touch = Input.GetTouch(0);
        Vector2 wp = Camera.main.ScreenToWorldPoint(touch.position);
        Collider2D[] hits = Physics2D.OverlapPointAll(wp);
        IsometricBuilding result = null;
        float lowestY = float.MaxValue;

        foreach (var h in hits)
        {
            var building = h.GetComponentInParent<IsometricBuilding>();
            if (building != null)
            {
                if (building == targetBuilding)
                {
                    result = targetBuilding;
                    break;
                }
                float y = building.transform.position.y;
                if (y < lowestY)
                {
                    lowestY = y;
                    result = building;
                }
            }
        }
        return result;
    }

    // Returns true if pointer is over any UI element in any canvas
    public static bool IsPointerOverAnyUI(Vector2 screenPosition)
    {
        // Get all active GraphicRaycasters
        var raycasters = FindObjectsByType<GraphicRaycaster>(sortMode:FindObjectsSortMode.None);
        foreach (var raycaster in raycasters)
        {
            // Only check enabled and active canvases
            if (!raycaster.enabled || !raycaster.gameObject.activeInHierarchy)
                continue;

            PointerEventData ped = new PointerEventData(EventSystem.current) { position = screenPosition };
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(ped, results);
            if (results.Count > 0)
                return true;
        }
        return false;
    }
}
