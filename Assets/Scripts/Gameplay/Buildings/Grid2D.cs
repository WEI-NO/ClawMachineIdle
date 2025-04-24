using CustomLibrary.References;
using UnityEngine;

public class Grid2D : MonoBehaviour
{
    public static Grid2D Instance;

    [Header("Grid Setting")]
    public float cellSize;


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
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.y / cellSize);
        return new Vector2Int(x, y);
    }

    #endregion grid
}
