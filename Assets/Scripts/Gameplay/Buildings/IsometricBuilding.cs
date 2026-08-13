using CustomLibrary.References;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public enum Orientation
{
    Left,
    Right
}

public class IsometricBuilding : MonoBehaviour
{
    [Header("Components")]
    private Animator anim;

    [Header("Building Info")]
    public string BuildingName;
    public string Suffix;
    public string BuildingID;

    [Header("Callbacks")]
    public Action<IsometricBuilding> OnPlaceableDestroy;

    [Header("Building Properties")]
    public IsometricBlueprint blueprint;
    public List<Transform> GridVisual;
    public List<ShaderInstancer> shaders;

    [Header("Outline Properties")]
    public float maxWidth = 5;
    public float selectedOutlineWidth = 1.0f;
    public float draggingOutlineWidth = 0.5f;
    public bool selected = false;

    [Header("Overlap Tint")]
    [SerializeField] private Color validTint = Color.white;
    [SerializeField] private Color overlapTint = new Color(1f, 0.55f, 0.55f, 1f); // slightly red
    private List<SpriteRenderer> spriteRenderers;
    private bool isDragging;

    [Header("Sorting")]
    [SerializeField] private SortingGroup sortingGroup;

    private void Awake()
    {
        shaders = new List<ShaderInstancer>();

        anim = GetComponent<Animator>();
        blueprint.SetOrientation(blueprint.currentOrientation);

        CacheShaderInstances();
        CacheSpriteRenderers();

        // A SortingGroup keeps this building's child sprites from interleaving
        // with another building's sprites; the manager sorts whole groups.
        if (sortingGroup == null)
        {
            sortingGroup = GetComponent<SortingGroup>();
        }
        if (sortingGroup == null)
        {
            sortingGroup = gameObject.AddComponent<SortingGroup>();
        }
    }

#if UNITY_EDITOR
    private void Reset()
    {
        sortingGroup = GetComponent<SortingGroup>();
    }
#endif

    private void Start()
    {

        SetOutline(false, 0.0f);
        IsometricSortingManager.Instance?.Register(this);
    }

    // Keep footprint dimensions positive when edited in the inspector (10.6).
    private void OnValidate()
    {
        blueprint.PixelDimension.x = Mathf.Max(1, blueprint.PixelDimension.x);
        blueprint.PixelDimension.y = Mathf.Max(1, blueprint.PixelDimension.y);
    }

    private void Update()
    {
#if UNITY_EDITOR
        DEBUG_TestIsometricGridInput();
#endif

        bool wasMoving = blueprint.TargetPosition != blueprint.GridPosition;

        if (wasMoving)
        {
            MoveTowardsTarget();
        }

        // Wall objects reorient themselves as they cross the room corner.
        if (blueprint.IsWallObject)
        {
            UpdateWallOrientation();
        }

        ApplyTransform();

        // Detect movement before LastGridPosition is overwritten, so sorting
        // rebuilds once in LateUpdate when the footprint's depth changed.
        bool sortingPositionChanged = blueprint.GridPosition != blueprint.LastGridPosition;

        blueprint.LastGridPosition = blueprint.GridPosition;

        if (sortingPositionChanged)
        {
            IsometricSortingManager.Instance?.MarkDirty();
        }

        // While being dragged in edit mode, tint red when overlapping another building.
        if (isDragging)
        {
            RefreshOverlapTint();
        }

        // The building has just come to rest at its target this frame: register
        // its footprint with the grid and run placement/collision checks.
        bool settledThisFrame = wasMoving && blueprint.TargetPosition == blueprint.GridPosition;
        if (settledThisFrame)
        {
            CommitPlacement();
        }
    }

    #region Movement & Placement

    // Remembers where occupancy was last reserved so we don't re-reserve or
    // re-log a collision every frame while the building sits still.
    private Vector2Int lastCommittedPosition;
    private bool hasCommitted;

    /// <summary>
    /// Advances the logical grid position one pixel-perfect step toward the
    /// target, preserving the original stepping / ramp-snapping feel. Validity
    /// is now asked of <see cref="IsometricGrid2D"/> through the footprint cells.
    /// </summary>
    private void MoveTowardsTarget()
    {
        Vector2Int start = blueprint.GridPosition;
        Vector2Int step = GetPixelPerfectStep(blueprint.TargetPosition - start);

        // 1. Preferred move: the pixel-perfect step toward the target.
        Vector2Int candidate = start + step;
        if (IsInBounds(candidate))
        {
            blueprint.GridPosition = candidate;
            return;
        }

        // 2. Fallback: nudge up a "ramp" along the movement axis, keeping each
        //    valid nudge (matches the original corrective snapping behaviour).
        Vector2Int nudged = start;
        bool nudgedIntoPlace = false;
        foreach (Vector2Int offset in GetRampOffsets(step))
        {
            Vector2Int next = nudged + offset;
            if (IsInBounds(next))
            {
                nudged = next;
                nudgedIntoPlace = true;
            }
        }
        if (nudgedIntoPlace)
        {
            blueprint.GridPosition = nudged;
            return;
        }

        // 3. Last resort: snap straight to the requested target if it fits.
        if (IsInBounds(blueprint.TargetPosition))
        {
            blueprint.GridPosition = blueprint.TargetPosition;
        }
        // Otherwise the building stays where it is this frame.
    }

    /// <summary>
    /// Halves the remaining distance but always moves at least one pixel per
    /// axis in the direction of travel. This is what keeps movement pixel-perfect.
    /// </summary>
    private static Vector2Int GetPixelPerfectStep(Vector2Int delta)
    {
        Vector2Int step = delta / 2;

        if (delta.x > 0) step.x = Mathf.Max(step.x, 1);
        else if (delta.x < 0) step.x = Mathf.Min(step.x, -1);

        if (delta.y > 0) step.y = Mathf.Max(step.y, 1);
        else if (delta.y < 0) step.y = Mathf.Min(step.y, -1);

        return step;
    }

    /// <summary>
    /// Corrective nudges tried, in order, when the direct step is blocked.
    /// Mirrors the original Right/Left/Top/Bottom ramp offsets.
    /// </summary>
    private static IEnumerable<Vector2Int> GetRampOffsets(Vector2Int step)
    {
        if (step.x > 0) yield return new Vector2Int(2, 0);
        else if (step.x < 0) yield return new Vector2Int(-2, 0);

        if (step.y > 0) yield return new Vector2Int(0, 1);
        else if (step.y < 0) yield return new Vector2Int(0, -1);
    }

    /// <summary>
    /// True when every cell the footprint would occupy at <paramref name="position"/>
    /// is inside the room. Ground and wall objects are checked against their own
    /// valid-cell set, since their footprints are calculated differently.
    /// </summary>
    private bool IsInBounds(Vector2Int position)
    {
        IsometricGrid2D grid = IsometricGrid2D.Instance;
        if (grid == null)
        {
            return false;
        }

        foreach (Vector2Int cell in GetOccupiedCells(position, blueprint.currentOrientation))
        {
            bool valid = blueprint.IsWallObject
                ? grid.IsValidWallCell(cell)
                : grid.IsValidGroundCell(cell);

            if (!valid)
            {
                return false;
            }
        }

        return true;
    }

    // Wall objects flip to face the correct room wall as they move past the corner.
    private void UpdateWallOrientation()
    {
        int moveDir = blueprint.GridPosition.x - blueprint.LastGridPosition.x;

        if (moveDir < 0 && blueprint.GridPosition.x - (blueprint.PixelDimension.x / 2) < 0)
        {
            SetFlip(Orientation.Right);
        }
        else if (moveDir > 0 && blueprint.GridPosition.x + (blueprint.PixelDimension.x / 2) > 0)
        {
            SetFlip(Orientation.Left);
        }
    }

    // Pushes the logical grid position onto the transform, if the cell is valid.
    private void ApplyTransform()
    {
        IsometricGrid2D grid = IsometricGrid2D.Instance;
        if (grid == null)
        {
            return;
        }

        if (grid.TryGetWorldPosition(blueprint.GridPosition, out Vector2 worldPos, blueprint.IsWallObject))
        {
            transform.position = worldPos;
        }
    }

    /// <summary>
    /// Reserves this building's footprint on the grid and reports overlaps.
    /// For now a collision is only logged; blocking / snapping can hook in here later.
    /// </summary>
    private void CommitPlacement()
    {
        IsometricGrid2D grid = IsometricGrid2D.Instance;
        if (grid == null)
        {
            return;
        }

        Vector2Int position = blueprint.GridPosition;

        // Nothing new to do if we already committed at this exact spot.
        if (hasCommitted && position == lastCommittedPosition)
        {
            return;
        }

        if (OverlapsOtherBuilding(position))
        {
            Debug.Log(
                $"{name} is colliding with / placed inside another building at {position}.",
                this);
        }

        // Clear any stale reservation, then reserve the new footprint when free.
        grid.Remove(this);
        grid.TryMove(this, position);

        lastCommittedPosition = position;
        hasCommitted = true;
    }

    /// <summary>
    /// True when the footprint fits in the room but is blocked by another
    /// building. The grid's <c>CanPlace</c> ignores this building's own cells,
    /// so a building never reports colliding with itself.
    /// </summary>
    private bool OverlapsOtherBuilding(Vector2Int position)
    {
        IsometricGrid2D grid = IsometricGrid2D.Instance;
        if (grid == null)
        {
            return false;
        }

        return IsInBounds(position)
            && !grid.CanPlace(this, position, blueprint.currentOrientation);
    }

    /// <summary>
    /// Called by the input controller when this building starts/stops being
    /// dragged in edit mode. Drives the overlap tint (red while overlapping).
    /// </summary>
    public void SetDragging(bool dragging)
    {
        isDragging = dragging;

        if (isDragging)
        {
            RefreshOverlapTint();
        }
        else
        {
            // Back to the normal look once the drag ends.
            SetVisualTint(validTint);
        }
    }

    // Tints the sprites red while the current position overlaps another building.
    private void RefreshOverlapTint()
    {
        bool overlapping = OverlapsOtherBuilding(blueprint.GridPosition);
        SetVisualTint(overlapping ? overlapTint : validTint);
    }

    private void SetVisualTint(Color color)
    {
        if (spriteRenderers == null)
        {
            return;
        }

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.color = color;
            }
        }
    }

    #endregion Movement & Placement

    #region Outline Control

    public void SetOutline(bool state, bool isDragging)
    {
        if (shaders == null || shaders.Count  <= 0) return;

        foreach (var s in shaders)
        {
            float target = state ? isDragging ? draggingOutlineWidth : selectedOutlineWidth : 0;
            target *= maxWidth;
            s.SetFloat(1, target);
        }
    }

    public void SetOutline(bool state, float widthPercentage)
    {
        if (shaders == null || shaders.Count <= 0) return;

        foreach (var s in shaders)
        {
            float target = state ? maxWidth * widthPercentage : 0.0f;
            s.SetFloat(1, target);
        }

    }

    #endregion outline control

    public void PlaceOnGridPosition(Vector2Int gridPos)
    {
        blueprint.TargetPosition = gridPos;
        blueprint.GridPosition = gridPos;

        ApplyTransform();

        blueprint.LastGridPosition = blueprint.GridPosition;

        // Register the footprint and run the placement/collision check.
        CommitPlacement();

        IsometricSortingManager.Instance?.MarkDirty();
    }

    public void SetSelected(bool state, bool isDragging)//, float outlineWidthPercentage = 1.0f)
    {
        if (state == selected) return;
        selected = state;

        SetOutline(state, isDragging);

        // Deselecting always ends drag styling and clears the overlap tint.
        if (!state)
        {
            SetDragging(false);
        }
    }

    public void PlayAnimation(string trigger)
    {
        if (anim != null)
        {
            anim.SetTrigger(trigger);
        }
    }

    public void ChangeVisualSize(float size)
    {
        if (GridVisual == null || GridVisual.Count <= 0) return;

        foreach (var gv in GridVisual)
        {
            gv.localScale = new Vector3(size, size, 1.0f);
        }
    }

    public void Move(Vector2Int gridPosition)
    {
        blueprint.GridPosition = gridPosition;
    }

    public void SetTargetPosition(Vector2Int targetPosition)
    {
        blueprint.TargetPosition = targetPosition;
    }

    /// <summary>
    /// Returns the perimeter cells this building would occupy at a candidate
    /// <paramref name="anchor"/>/<paramref name="orientation"/>, without mutating
    /// live state (5.1 / 5.2). Used by the grid for occupancy checks.
    /// </summary>
    public IEnumerable<Vector2Int> GetOccupiedCells(Vector2Int anchor, Orientation orientation)
    {
        return blueprint.GetOccupiedCells(anchor, orientation);
    }

    /// <summary>
    /// Filled footprint cells at a candidate position. Used by the grid for
    /// occupancy / overlap detection (interior included, not just the outline).
    /// </summary>
    public IEnumerable<Vector2Int> GetFilledCells(Vector2Int anchor, Orientation orientation)
    {
        return blueprint.GetFilledCells(anchor, orientation);
    }

    #region Sorting

    /// <summary>
    /// The building's ground footprint as a range on the two isometric floor
    /// axes. Uses the perimeter corners (convex, so min/max fall on the outline).
    /// </summary>
    public IsometricDepthBounds GetDepthBounds()
    {
        int minIsoX = int.MaxValue;
        int maxIsoX = int.MinValue;
        int minIsoY = int.MaxValue;
        int maxIsoY = int.MinValue;

        foreach (Vector2Int corner in GetOccupiedCells(blueprint.GridPosition, blueprint.currentOrientation))
        {
            Vector2Int isoPoint = IsometricDepthBounds.GridToIsoAxes(corner);

            minIsoX = Mathf.Min(minIsoX, isoPoint.x);
            maxIsoX = Mathf.Max(maxIsoX, isoPoint.x);
            minIsoY = Mathf.Min(minIsoY, isoPoint.y);
            maxIsoY = Mathf.Max(maxIsoY, isoPoint.y);
        }

        return new IsometricDepthBounds(minIsoX, maxIsoX, minIsoY, maxIsoY);
    }

    // Applied by the sorting manager; drives the whole building's draw order.
    public void SetSortingOrder(int sortingOrder)
    {
        if (sortingGroup != null)
        {
            sortingGroup.sortingOrder = sortingOrder;
        }
    }

    #endregion Sorting

    private void DEBUG_TestIsometricGridInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            blueprint.GridPosition = blueprint.GridPosition + new Vector2Int(0, 1);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            blueprint.GridPosition = blueprint.GridPosition + new Vector2Int(0, -1);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            blueprint.GridPosition = blueprint.GridPosition + new Vector2Int(-1, 0);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            blueprint.GridPosition = blueprint.GridPosition + new Vector2Int(1, 0);
        }
    }

    // Draw the full occupied footprint when selected in the editor (18.1).
    private void OnDrawGizmosSelected()
    {
        if (IsometricGrid2D.Instance == null)
        {
            return;
        }

        const float cellSize = 0.03125f;
        Gizmos.color = Color.red;

        // Draw the filled footprint so occupancy / overlap coverage is visible.
        foreach (Vector2Int cell in blueprint.GetFilledCells(blueprint.GridPosition, blueprint.currentOrientation))
        {
            if (IsometricGrid2D.Instance.TryGetWorldPosition(cell, out Vector2 wPos, blueprint.IsWallObject))
            {
                Gizmos.DrawWireCube((Vector3)wPos, Vector3.one * cellSize);
            }
        }
    }

    public void DestroyPlaceable()
    {
        // Release occupancy before the object goes away (11.3).
        if (IsometricGrid2D.Instance != null)
        {
            IsometricGrid2D.Instance.Remove(this);
        }

        OnPlaceableDestroy?.Invoke(this);
        Destroy(gameObject);
    }

    // Safety fallback so occupancy is released even on unexpected destruction.
    private void OnDestroy()
    {
        IsometricSortingManager.Instance?.Unregister(this);

        if (IsometricGrid2D.Instance != null)
        {
            IsometricGrid2D.Instance.Remove(this);
        }
    }

    #region Rotation

    private const float flippedYRot = 180.0f;
    private const float normalYRot = 0;

    public void Flip()
    {
        int or = blueprint.currentOrientation.ToInt();
        or = (or + 1) % 2;
        SetFlip((Orientation)or);
    }

    public void SetFlip(Orientation orientation)
    {
        if (GridVisual != null && GridVisual.Count > 0)
        {
            foreach (var gv in GridVisual)
            {
                Vector3 rot = gv.transform.localRotation.eulerAngles;
                float yRot = orientation == Orientation.Right ? normalYRot : flippedYRot;
                rot.y = yRot;
                gv.transform.localRotation = Quaternion.Euler(rot.x, rot.y, rot.z);
            }

        }
        blueprint.SetOrientation(orientation);

        IsometricSortingManager.Instance?.MarkDirty();
    }


    #endregion rotation

    #region Helper

    private void CacheShaderInstances()
    {
        shaders = new List<ShaderInstancer>();

        if (GridVisual == null)
        {
            return;
        }

        foreach (Transform gridVisual in GridVisual)
        {
            if (gridVisual == null)
            {
                continue;
            }

            ShaderInstancer shaderInstance =
                gridVisual.GetComponent<ShaderInstancer>();

            if (shaderInstance != null)
            {
                shaders.Add(shaderInstance);
            }
        }
    }

    // Caches every SpriteRenderer under the GridVisual objects so the whole
    // building can be tinted together (e.g. red while overlapping).
    private void CacheSpriteRenderers()
    {
        spriteRenderers = new List<SpriteRenderer>();

        if (GridVisual == null)
        {
            return;
        }

        foreach (Transform gridVisual in GridVisual)
        {
            if (gridVisual == null)
            {
                continue;
            }

            spriteRenderers.AddRange(gridVisual.GetComponentsInChildren<SpriteRenderer>(true));
        }
    }

    #endregion
}

public enum IsometricCorner
{
    Top_L,
    Top_R,
    Right_T,
    Right_B,
    Bottom_L,
    Bottom_R,
    Left_B,
    Left_T
}

// Keeps track of the corners of the isometric buildings
[System.Serializable]
public struct IsometricBlueprint
{
    [Header("Blueprint Properties")]
    public Vector2Int PixelDimension; // Dimension in pixel.
    public Vector2Int _currentDimension;

    public Vector2Int _lastGridPosition;
    public Vector2Int _gridPosition;
    public Vector2Int TargetPosition;

    public Orientation currentOrientation;

    public bool IsWallObject;

    #region Getter/Setter

    public Vector2Int GridPosition { get { return _gridPosition; } set { _gridPosition = value; } }
    public Vector2Int LastGridPosition { get { return _lastGridPosition; } set { _lastGridPosition = value; } }

    #endregion getter/setter

    /// <summary>
    /// Footprint dimensions for a given orientation. Left rotates the base
    /// pixel dimensions (10.3). Independent of current position.
    /// </summary>
    public Vector2Int GetDimensions(Orientation orientation)
    {
        return orientation == Orientation.Left
            ? new Vector2Int(PixelDimension.y, PixelDimension.x)
            : PixelDimension;
    }

    /// <summary>
    /// Perimeter (outline) cells of the footprint at an arbitrary
    /// <paramref name="anchor"/>/<paramref name="orientation"/> without touching
    /// <see cref="_gridPosition"/> (5.2). Used for room-boundary checks.
    /// </summary>
    public IEnumerable<Vector2Int> GetOccupiedCells(Vector2Int anchor, Orientation orientation)
    {
        return GetFootprintCorners(anchor, orientation);
    }

    /// <summary>
    /// Every cell inside the isometric footprint (filled, not just the outline).
    /// Used for occupancy / overlap so a building placed fully inside another is
    /// detected, not only edge overlaps.
    /// </summary>
    public IEnumerable<Vector2Int> GetFilledCells(Vector2Int anchor, Orientation orientation)
    {
        return FillConvexFootprint(GetFootprintCorners(anchor, orientation));
    }

    /// <summary>
    /// The footprint's corner cells in perimeter (winding) order, so consecutive
    /// pairs form the polygon edges. Ground objects are an octagon, walls a quad.
    /// </summary>
    private Vector2Int[] GetFootprintCorners(Vector2Int anchor, Orientation orientation)
    {
        Vector2Int dim = GetDimensions(orientation);

        if (!IsWallObject)
        {
            Vector2Int bottomL = anchor - new Vector2Int(2, 0);
            Vector2Int bottomR = anchor + new Vector2Int(1, 0);
            Vector2Int leftB = anchor + new Vector2Int(-dim.x, dim.x / 2 - 1);
            Vector2Int rightB = anchor + new Vector2Int(dim.y - 1, dim.y / 2 - 1);

            int height = (dim.x + dim.y) / 2 - 1;
            int rightSkew = Mathf.Abs(dim.y - dim.x) * (orientation == Orientation.Left ? -1 : 1);

            Vector2Int leftT = leftB + new Vector2Int(0, 1);
            Vector2Int rightT = rightB + new Vector2Int(0, 1);
            Vector2Int topL = bottomL + new Vector2Int(rightSkew, height);
            Vector2Int topR = bottomR + new Vector2Int(rightSkew, height);

            // Counter-clockwise around the octagon.
            return new[] { bottomL, leftB, leftT, topL, topR, rightT, rightB, bottomR };
        }
        else
        {
            int height = Mathf.CeilToInt(dim.x / 2.0f) - 1;

            Vector2Int bottomL = orientation == Orientation.Right
                ? anchor
                : anchor + new Vector2Int(-(dim.x - 1), height);
            Vector2Int bottomR = orientation == Orientation.Left
                ? anchor
                : anchor + new Vector2Int(dim.x - 1, height);

            Vector2Int topL = bottomL + new Vector2Int(0, dim.y - 1);
            Vector2Int topR = bottomR + new Vector2Int(0, dim.y - 1);

            // Counter-clockwise around the wall quad.
            return new[] { bottomL, topL, topR, bottomR };
        }
    }

    /// <summary>
    /// Scanline-fills the convex polygon defined by <paramref name="corners"/>.
    /// Edges are straight lines, so linear interpolation exactly follows the
    /// isometric 2:1 slopes encoded in the corner coordinates.
    /// </summary>
    private static IEnumerable<Vector2Int> FillConvexFootprint(Vector2Int[] corners)
    {
        int yMin = int.MaxValue;
        int yMax = int.MinValue;
        for (int i = 0; i < corners.Length; i++)
        {
            if (corners[i].y < yMin) yMin = corners[i].y;
            if (corners[i].y > yMax) yMax = corners[i].y;
        }

        for (int y = yMin; y <= yMax; y++)
        {
            float xLeft = float.MaxValue;
            float xRight = float.MinValue;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector2Int a = corners[i];
                Vector2Int b = corners[(i + 1) % corners.Length];

                bool spansScanline = (a.y <= y && b.y >= y) || (b.y <= y && a.y >= y);
                if (!spansScanline)
                {
                    continue;
                }

                if (a.y == b.y)
                {
                    // Horizontal edge lying on this scanline.
                    xLeft = Mathf.Min(xLeft, Mathf.Min(a.x, b.x));
                    xRight = Mathf.Max(xRight, Mathf.Max(a.x, b.x));
                }
                else
                {
                    float t = (float)(y - a.y) / (b.y - a.y);
                    float x = a.x + t * (b.x - a.x);
                    xLeft = Mathf.Min(xLeft, x);
                    xRight = Mathf.Max(xRight, x);
                }
            }

            if (xLeft > xRight)
            {
                continue;
            }

            // Ceil/Floor keeps the fill inside the true edges.
            int xStart = Mathf.CeilToInt(xLeft);
            int xEnd = Mathf.FloorToInt(xRight);
            for (int x = xStart; x <= xEnd; x++)
            {
                yield return new Vector2Int(x, y);
            }
        }
    }


    public Vector2Int GetCornerPosition(IsometricCorner corner)
    {
        // Floor objects
        if (!IsWallObject)
        {
            switch (corner)
            {
                case IsometricCorner.Top_L:
                    return GetTop_LCorner();
                case IsometricCorner.Top_R:
                    return GetTop_RCorner();
                case IsometricCorner.Right_T:
                    return GetRight_TCorner();
                case IsometricCorner.Right_B:
                    return GetRight_BCorner();
                case IsometricCorner.Bottom_L:
                    return GetBottom_LCorner();
                case IsometricCorner.Bottom_R:
                    return GetBottom_RCorner();
                case IsometricCorner.Left_B:
                    return GetLeft_BCorner();
                default: // IsometricCorner.Left_T:
                    return GetLeft_TCorner();
            }
        } 
        // Wall object
        else
        {
            switch(corner)
            {
                case IsometricCorner.Top_L:
                    return GetTop_LCorner();
                case IsometricCorner.Top_R:
                    return GetTop_RCorner();
                case IsometricCorner.Bottom_L:
                    return GetBottom_LCorner();
                case IsometricCorner.Bottom_R:
                    return GetBottom_RCorner();
                default:
                    // Wall footprints only support the four corners above (10.5).
                    Debug.LogError($"Unsupported wall corner: {corner}");
                    return GetTop_LCorner();
            }
        }

    }


    #region Isometric Corners

    // Function stubs for each corner
    private Vector2Int GetTop_LCorner()
    {
        if (!IsWallObject)
        {
            Vector2Int bottomLeft = GetBottom_LCorner();
            int height = (_currentDimension.x + _currentDimension.y) / 2; height -= 1;

            int higher = _currentDimension.y > _currentDimension.x ? _currentDimension.y : _currentDimension.x;
            int lower = _currentDimension.y < _currentDimension.x ? _currentDimension.y : _currentDimension.x;
            int rightSkew = higher - lower;

            return bottomLeft + new Vector2Int(rightSkew * (currentOrientation == Orientation.Left ? -1 : 1), height);
        } else
        {
            return GetBottom_LCorner() + new Vector2Int(0, _currentDimension.y - 1);
        }

    }

    private Vector2Int GetTop_RCorner()
    {
        if (!IsWallObject)
        {
            Vector2Int bottomRight = GetBottom_RCorner();
            int height = (_currentDimension.x + _currentDimension.y) / 2; height -= 1;

            int higher = _currentDimension.y > _currentDimension.x ? _currentDimension.y : _currentDimension.x;
            int lower = _currentDimension.y < _currentDimension.x ? _currentDimension.y : _currentDimension.x;
            int rightSkew = higher - lower;

            return bottomRight + new Vector2Int(rightSkew * (currentOrientation == Orientation.Left ? -1 : 1), height);
        } else
        {
            return GetBottom_RCorner() + new Vector2Int(0, _currentDimension.y - 1);
        }
    }

    private Vector2Int GetRight_TCorner()
    {
        return GetRight_BCorner() + new Vector2Int(0, 1);
    }

    private Vector2Int GetRight_BCorner()
    {
        int heightTravelled = (_currentDimension.y / 2) - 1;
        return _gridPosition + new Vector2Int(_currentDimension.y - 1, heightTravelled);
    }

    private Vector2Int GetBottom_LCorner()
    {
        if (!IsWallObject)
            return _gridPosition - new Vector2Int(2, 0); // Move grid position 2 pixels to the left
        else
        {
            if (currentOrientation == Orientation.Right)
                return _gridPosition;
            else
            {
                int height = Mathf.CeilToInt(_currentDimension.x / 2.0f) - 1;
                return _gridPosition + new Vector2Int(-(_currentDimension.x - 1), height);
            }
        }

    }

    private Vector2Int GetBottom_RCorner()
    {
        if (!IsWallObject)
            return _gridPosition + new Vector2Int(1, 0); // Move grid position 1 pixel to the right
        else
        {
            if (currentOrientation == Orientation.Left)
                return _gridPosition;
            else
            {
                int height = Mathf.CeilToInt(_currentDimension.x / 2.0f) - 1;
                return _gridPosition + new Vector2Int(_currentDimension.x - 1, height);
            }
        }

    }

    private Vector2Int GetLeft_BCorner()
    {
        int heightTravelled = _currentDimension.x / 2 - 1;
        return _gridPosition + new Vector2Int(-_currentDimension.x, heightTravelled);
    }

    private Vector2Int GetLeft_TCorner()
    {
        return GetLeft_BCorner() + new Vector2Int(0, 1);
    }

    #endregion isometric corners

    #region Flip

    public void SetOrientation(Orientation or)
    {
        currentOrientation = or;
        Vector2Int flippedDimension = currentOrientation == Orientation.Left ? new Vector2Int(PixelDimension.y, PixelDimension.x) : PixelDimension;
        _currentDimension = flippedDimension;
    }

    #endregion flip



}
