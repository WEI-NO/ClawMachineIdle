using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

public class IsometricBuilding : MonoBehaviour
{
    [Header("Building Properties")]
    public IsometricBlueprint blueprint;

    private void Update()
    {
        DEBUG_TestIsometricGridInput();

        if (blueprint.LastGridPosition != blueprint.GridPosition)
        {
            Vector2Int direction = blueprint.GridPosition - blueprint.LastGridPosition;

            List<Vector2Int> testPoints = new List<Vector2Int>();

            if (direction.x > 0) // Right
            {
                // Moved right
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Bottom_R));
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Right_B));
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Right_T));
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Top_R));
            }
            else if (direction.x < 0) // Left
            {
                // Moved left
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Bottom_L));
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Left_B));
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Left_T));
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Top_L));
            }

            if (direction.y > 0) // Up
            {
                // Moved up
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Left_T));
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Top_L));
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Top_R));
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Right_T));
            }
            else if (direction.y < 0) // Down
            {
                // Moved down
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Left_B));
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Bottom_L));
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Bottom_R));
                testPoints.Add(blueprint.GetCornerPosition(IsometricCorner.Right_B));
            }

            bool outOfBound = false;
            foreach (var point in testPoints)
            {
                if (!IsometricGrid2D.Instance.GetWorldPosition(point, out Vector2 w))
                {
                    // If it is out of bound.
                    outOfBound = true;
                    break;
                }
            }

            if (outOfBound)
            {
                blueprint.GridPosition = blueprint.LastGridPosition;
            }

            // If it is not out of bound, apply the change
            IsometricGrid2D.Instance.GetWorldPosition(blueprint.GridPosition, out Vector2 wp);
            transform.position = wp;
        }

    }

    private void DEBUG_TestIsometricGridInput()
    {
        blueprint.LastGridPosition = blueprint.GridPosition;
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

    private Vector2Int _lastGridPosition;
    private Vector2Int _gridPosition;

    #region Getter/Setter

    public Vector2Int GridPosition { get { return _gridPosition; } set { _gridPosition = value; } }
    public Vector2Int LastGridPosition { get { return _lastGridPosition; } set { _lastGridPosition = value; } }

    #endregion getter/setter


    public Vector2Int GetCornerPosition(IsometricCorner corner)
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


    #region Isometric Corners

    // Function stubs for each corner
    private Vector2Int GetTop_LCorner()
    {
        return GetBottom_LCorner() + new Vector2Int(0, PixelDimension.y - 1);
    }

    private Vector2Int GetTop_RCorner()
    {
        return GetBottom_RCorner() + new Vector2Int(0, PixelDimension.y - 1);
    }

    private Vector2Int GetRight_TCorner()
    {
        return GetRight_BCorner() + new Vector2Int(0, 1);
    }

    private Vector2Int GetRight_BCorner()
    {
        int heightTravelled = (PixelDimension.x / 2) - 1;
        return _gridPosition + new Vector2Int(PixelDimension.x - 1, heightTravelled);
    }

    private Vector2Int GetBottom_LCorner()
    {
        return _gridPosition - new Vector2Int(2, 0); // Move grid position 2 pixels to the left
    }

    private Vector2Int GetBottom_RCorner()
    {
        return _gridPosition + new Vector2Int(1, 0); // Move grid position 1 pixel to the right
    }

    private Vector2Int GetLeft_BCorner()
    {
        int heightTravelled = PixelDimension.x / 2;
        return _gridPosition + new Vector2Int(-PixelDimension.x, heightTravelled);
    }

    private Vector2Int GetLeft_TCorner()
    {
        return GetLeft_BCorner() + new Vector2Int(0, -1);
    }

    #endregion isometric corners

}
