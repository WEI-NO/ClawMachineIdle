using CustomLibrary.References;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

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

    [Header("Building Properties")]
    public IsometricBlueprint blueprint;
    public List<Transform> GridVisual;
    public List<ShaderInstancer> shader;

    [Header("Outline Properties")]
    public float maxWidth = 5;
    public float selectedOutlineWidth = 1.0f;
    public float draggingOutlineWidth = 0.5f;
    public bool selected = false;

    private void Awake()
    {
        shader = new List<ShaderInstancer>();
        anim = GetComponent<Animator>();
        if (GridVisual != null || GridVisual.Count > 0)
        {
            foreach (var gv in GridVisual)
            {
                shader.Add(gv.GetComponent<ShaderInstancer>());

                if (gv.transform.rotation.eulerAngles.y == 0)
                {
                    blueprint.SetOrientation(Orientation.Right);
                }
                else
                {
                    blueprint.SetOrientation(Orientation.Left);
                }
            }


        }

    }

    private void Start()
    {
        SetOutline(false, 0.0f);
    }

    private void Update()
    {
        DEBUG_TestIsometricGridInput();
        if (blueprint.TargetPosition != blueprint.GridPosition)
        {
            Vector2Int direction = blueprint.TargetPosition - blueprint.GridPosition;
            Vector2Int result = direction / 2;

            // Clamp each component separately
            if (direction.x > 0)
                result.x = Mathf.Max(result.x, 1);
            else if (direction.x < 0)
                result.x = Mathf.Min(result.x, -1);
            // if result.x == 0, leave it as 0

            if (direction.y > 0)
                result.y = Mathf.Max(result.y, 1);
            else if (direction.y < 0)
                result.y = Mathf.Min(result.y, -1);

            Vector2Int targetPosition = blueprint.GridPosition + result;

            bool foundValidPlacement = false;
            if (blueprint.IsWallObject)
            {
                if (!ValidPlacement_Wall(targetPosition, blueprint.GridPosition, out List<IsometricCorner> directions))
                {
                    blueprint.GridPosition = blueprint.LastGridPosition;
                    // Try pixel snapping up a ramp
                    Vector2Int pixelPerfectOffset = new Vector2Int();
                    foreach (var p in directions)
                    {
                        pixelPerfectOffset = Vector2Int.zero;
                        switch (p)
                        {
                            case IsometricCorner.Right_B:
                                pixelPerfectOffset.x = 2;
                                break;
                            case IsometricCorner.Left_B:
                                pixelPerfectOffset.x = -2;
                                break;
                            case IsometricCorner.Top_L:
                                pixelPerfectOffset.y = 1;
                                break;
                            case IsometricCorner.Bottom_L:
                                pixelPerfectOffset.y = -1;
                                break;
                        }
                        if (pixelPerfectOffset != Vector2Int.zero)
                        {
                            targetPosition = blueprint.GridPosition + pixelPerfectOffset;
                            if (!ValidPlacement_Wall(targetPosition, blueprint.GridPosition, out List<IsometricCorner> dir))
                            {
                                blueprint.GridPosition = blueprint.LastGridPosition;
                            }
                            else
                            {
                                blueprint.GridPosition = targetPosition;
                                foundValidPlacement = true;
                            }
                        } else
                        {
                            break;
                        }
                    }
                }
            } 
            else
            {
                if (!ValidPlacement(targetPosition, blueprint.GridPosition, out List<IsometricCorner> directions))
                {
                    blueprint.GridPosition = blueprint.LastGridPosition;
                    // Try pixel snapping up a ramp
                    Vector2Int pixelPerfectOffset = new Vector2Int();

                    foreach (var p in directions)
                    {
                        pixelPerfectOffset = Vector2Int.zero;
                        switch (p)
                        {
                            case IsometricCorner.Right_B:
                                pixelPerfectOffset.x = 2;
                                break;
                            case IsometricCorner.Left_B:
                                pixelPerfectOffset.x = -2;
                                break;
                            case IsometricCorner.Top_L:
                                pixelPerfectOffset.y = 1;
                                break;
                            case IsometricCorner.Bottom_L:
                                pixelPerfectOffset.y = -1;
                                break;
                        }
                        if (pixelPerfectOffset != Vector2Int.zero)
                        {
                            targetPosition = blueprint.GridPosition + pixelPerfectOffset;
                            if (!ValidPlacement(targetPosition, blueprint.GridPosition, out List<IsometricCorner> dir))
                            {
                                blueprint.GridPosition = blueprint.LastGridPosition;
                            }
                            else
                            {
                                blueprint.GridPosition = targetPosition;
                                foundValidPlacement = true;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    blueprint.GridPosition = targetPosition;
                    foundValidPlacement = true;
                }
            }

            // If getting the half point isn't possible, try the target point
            if (!foundValidPlacement)
            {
                if (!blueprint.IsWallObject ? 
                    ValidPlacement(blueprint.TargetPosition, blueprint.GridPosition, out List<IsometricCorner> dir1) :
                    ValidPlacement_Wall(blueprint.TargetPosition, blueprint.GridPosition, out List<IsometricCorner> dir2))
                {
                    blueprint.GridPosition = blueprint.TargetPosition;
                }
            }
        }


        // Auto flip wall objects when past a certain threshold
        if (blueprint.IsWallObject)
        {
            int moveDir = blueprint.GridPosition.x - blueprint.LastGridPosition.x;

            if (moveDir < 0)
            {
                if (blueprint.GridPosition.x - (blueprint.PixelDimension.x / 2) < 0)
                {
                    SetFlip(Orientation.Right);
                }
            }

            if (moveDir > 0)
            {

                if (blueprint.GridPosition.x + (blueprint.PixelDimension.x / 2) > 0)
                {
                    SetFlip(Orientation.Left);
                }
            }
        }


        // If it is not out of bound, apply the change
        IsometricGrid2D.Instance.GetWorldPosition(blueprint.GridPosition, out Vector2 wp, blueprint.IsWallObject);
        transform.position = wp;

        blueprint.LastGridPosition = blueprint.GridPosition;
    }

    #region Outline Control

    public void SetOutline(bool state, bool isDragging)
    {
        if (shader == null || shader.Count  <= 0) return;

        foreach (var s in shader)
        {
            float target = state ? isDragging ? draggingOutlineWidth : selectedOutlineWidth : 0;
            target *= maxWidth;
            s.SetFloat(1, target);
        }
    }

    public void SetOutline(bool state, float widthPercentage)
    {
        if (shader == null || shader.Count <= 0) return;

        foreach (var s in shader)
        {
            float target = state ? maxWidth * widthPercentage : 0.0f;
            s.SetFloat(1, target);
        }

    }

    #endregion outline control

    public void SetSelected(bool state, bool isDragging)//, float outlineWidthPercentage = 1.0f)
    {
        if (state == selected) return;
        selected = state;

        SetOutline(state, isDragging);
    }

    public void PlayAnimation(string trigger)
    {
        anim.SetTrigger(trigger);
    }

    public void ChangeVisualSize(float size)
    {
        if (GridVisual == null || GridVisual.Count <= 0) return;

        foreach (var gv in GridVisual)
        {
            gv.localScale = new Vector3(size, size, 1.0f);
        }
    }

    public bool ValidPlacement_Wall(Vector2Int coord, Vector2Int current, out List<IsometricCorner> directions)
    {
        Vector2Int direction = coord - current;
        //direction = new Vector2Int(direction.x != 0 ? direction.x / Mathf.Abs(direction.x) : 0, direction.y != 0 ? direction.y / Mathf.Abs(direction.y) : 0);
        directions = new List<IsometricCorner>();
        List<Vector2Int> testPoints = new List<Vector2Int>();

        if (direction.x > 0) // Right
        {
            // Moved right
            directions.Add(IsometricCorner.Right_B);
        }
        else if (direction.x < 0) // Left
        {
            // Moved left
            directions.Add(IsometricCorner.Left_B);
        }

        if (direction.y > 0) // Up
        {
            // Moved up
            directions.Add(IsometricCorner.Top_L);
        }
        else if (direction.y < 0) // Down
        {
            // Moved down
            directions.Add(IsometricCorner.Bottom_L);
        }

        testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Bottom_L));
        testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Bottom_R));
        testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Top_L));
        testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Top_R));


        bool outOfBound = false;
        foreach (var point in testPoints)
        {
            if (!IsometricGrid2D.Instance.GetWorldPosition(point + direction, out Vector2 w, true))
            {
                // If it is out of bound.
                outOfBound = true;
                break;
            }
        }

        return !outOfBound;
    }

    public bool ValidPlacement(Vector2Int coord, Vector2Int current, out List<IsometricCorner> directions)
    {
        Vector2Int direction = coord - current;
        //direction = new Vector2Int(direction.x != 0 ? direction.x / Mathf.Abs(direction.x) : 0, direction.y != 0 ? direction.y / Mathf.Abs(direction.y) : 0);
        directions = new List<IsometricCorner>();
        List<Vector2Int> testPoints = new List<Vector2Int>();


        if (direction.x > 0) // Right
        {
            // Moved right
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Bottom_R));
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Right_B));
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Right_T));
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Top_R));
            directions.Add(IsometricCorner.Right_B);
        }
        else if (direction.x < 0) // Left
        {
            // Moved left
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Bottom_L));
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Left_B));
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Left_T));
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Top_L));
            directions.Add(IsometricCorner.Left_B);
        }

        if (direction.y > 0) // Up
        {
            // Moved up
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Left_T));
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Top_L));
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Top_R));
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Right_T));
            directions.Add(IsometricCorner.Top_L);
        }
        else if (direction.y < 0) // Down
        {
            // Moved down
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Left_B));
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Bottom_L));
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Bottom_R));
            testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Right_B));
            directions.Add(IsometricCorner.Bottom_L);
        }

        bool outOfBound = false;
        foreach (var point in testPoints)
        {
            if (!IsometricGrid2D.Instance.GetWorldPosition(point + direction, out Vector2 w))
            {
                // If it is out of bound.
                outOfBound = true;
                break;
            }
        }

        return !outOfBound;
    }

    public void Move(Vector2Int gridPosition)
    {
        blueprint.GridPosition = gridPosition;
    }

    public void SetTargetPosition(Vector2Int targetPosition)
    {
        blueprint.TargetPosition = targetPosition;
    }

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

    // Draw gizmos for all corners when selected in editor
    private void OnDrawGizmosSelected()
    {
        if (IsometricGrid2D.Instance == null)
        {
            return;
        }

        // Radius for gizmo dots
        float radius = 0.01f;
        Gizmos.color = Color.red;

        foreach (IsometricCorner corner in Enum.GetValues(typeof(IsometricCorner)))
        {
            Vector2Int cornerGrid = blueprint.GetCornerPosition(corner);

            bool result = IsometricGrid2D.Instance.GetWorldPosition(cornerGrid, out Vector2 wPos);
            // Convert grid to world if needed
            Vector3 worldPos = (Vector3)wPos;// - new Vector3(0, 0.03125f * 9.0f, 0);

            // Draw the sphere
            Gizmos.DrawSphere(worldPos, radius);
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
    }


    #endregion rotation
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
//if (blueprint.IsWallObject)
//{
//    if (blueprint.currentOrientation == Orientation.Right)
//    {
//        int flipTestPosition = blueprint.GridPosition.x + blueprint.PixelDimension.x;
//        if (flipTestPosition > 0)
//        {
//            blueprint.GridPosition = new Vector2Int(flipTestPosition +  (blueprint.PixelDimension.x / 2), blueprint.GridPosition.y);
//            SetFlip(Orientation.Left);
//        }
//    } 
//    else // blueprint.currentOrientation == Orientation.Left
//    {
//        int flipTestPosition = blueprint.GridPosition.x - blueprint.PixelDimension.x;
//        if (flipTestPosition < -1)
//        {
//            blueprint.GridPosition = new Vector2Int(flipTestPosition - (blueprint.PixelDimension.x / 2), blueprint.GridPosition.y);
//            SetFlip(Orientation.Right);
//        }
//    }
//}