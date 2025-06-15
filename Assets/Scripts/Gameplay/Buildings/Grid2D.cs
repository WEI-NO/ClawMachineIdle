using CustomLibrary.References;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public enum GridType
{
    Wall,
    Floor
}

public class Grid2D : MonoBehaviour
{
    public static Grid2D Instance;

    [Header("Grid Setting")]
    public float cellSize;

    [Header("Grid Information")]
    public Dictionary<Vector2Int, GridTile2D> GridObjects;


    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    #region Grid

    /// <summary>
    /// Convert a world position into grid position. Side Note: Works if the building sprite's pivot is set to bottom left.
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns>Grid Position</returns>
    public Vector2Int GetGridPosition(Vector2 worldPosition)
    {
        float isoX = worldPosition.x / cellSize;
        float isoY = worldPosition.y / (cellSize / 2);

        int x = Mathf.FloorToInt((isoY + isoX) / 2f);
        int y = Mathf.FloorToInt((isoY - isoX) / 2f);

        return new Vector2Int(x, y);
    }

    public Vector2 GetWorldPosition(Vector2Int gridPosition)
    {
        float x = (gridPosition.x - gridPosition.y) * (cellSize / 2f);
        float y = (gridPosition.x + gridPosition.y) * (cellSize / 4f); // Adjust divisor to match your isometric tile ratio

        return new Vector2(x, y);
    }

    #endregion grid

    #region Grid Objects

    /// <summary>
    /// Runs a validation check before placing the object.
    /// Also sets the parentTile flag.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>Success of placement</returns>
    public bool PlaceObject(BaseGridObject obj)
    {
        if (!ValidateObject(obj))
        {
            return false;
        }

        Vector2Int bottomLeft = obj.GridPosition;
        Vector2Int topRight = bottomLeft + obj._ObjectDimension;
        GridObjects[bottomLeft].ApplyObject(obj, true); // Set is parent

        for (int i = bottomLeft.x; i < topRight.x; i++)
        {
            for (int j = bottomLeft.y + 1; j < topRight.y; j++) // Skips the first element (Parent).
            {
                Vector2Int key = new Vector2Int(i, j);
                GridObjects[key].ApplyObject(obj);
            }
        }
        return true;
    }

    /// <summary>
    /// Validates whether a building can be placed on a specific tile, taking dimension into account.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>Whether it can be placed.</returns>
    public bool ValidateObject(BaseGridObject obj)
    {
        Vector2Int bottomLeft = obj.GridPosition;
        Vector2Int topRight = bottomLeft + obj._ObjectDimension;

        for (int i = bottomLeft.x; i < topRight.x; i++)
        {
            for (int j = bottomLeft.y; j < topRight.y; j++)
            {
                Vector2Int key = new Vector2Int(i, j);
                if (GridObjects.TryGetValue(key, out var val))
                {
                    if (!val.Compatible(obj) || val.containedObject != null)
                    {
                        return false;
                    }
                } else
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Generates a box and set its type.
    /// </summary>
    /// <param name="upperLeft"></param>
    /// <param name="bottomRight"></param>
    /// <param name="type"></param>
    public void CreateBox(Vector2Int upperLeft, Vector2Int bottomRight, GridType type)
    {
        for (int i = upperLeft.x; i <= bottomRight.x; i++)
        {
            for (int j = upperLeft.y; j <= bottomRight.y; j--)
            {
                Vector2Int key = new Vector2Int(i, j);
                GridObjects[key] = new GridTile2D(type);
            }
        }
    }


    /// <summary>
    /// Clears the grid objects to be empty.
    /// </summary>
    public void ClearGridObjects()
    {
        GridObjects.Clear();
    }

    #endregion grid objects
}

/// <summary>
/// Used by Grid2D, and holds information about current objects.
/// </summary>
public struct GridTile2D
{
    [Header("Tile Information")]
    public GridType type;
    public BaseGridObject containedObject;
    public bool isParent;

    public GridTile2D(GridType type, bool parent = false)
    {
        this.type = type;
        containedObject = null;
        isParent = parent;
    }

    public bool Compatible(BaseGridObject obj)
    {
        if (obj == null) return false;

        return obj.occupiedType == type;
    }

    public bool ApplyObject(BaseGridObject obj, bool isParent = false)
    {
        if (!Compatible(obj) || obj == null)
        {
            return false;
        }
        this.isParent = isParent;
        containedObject = obj;
        return true;
    }
}
