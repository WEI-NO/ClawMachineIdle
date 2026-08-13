using UnityEngine;

/// <summary>
/// A building's ground footprint expressed as a range on the two independent
/// isometric floor axes. Comparing full ranges (instead of a single pivot)
/// preserves building size when sorting differently sized buildings.
/// </summary>
public readonly struct IsometricDepthBounds
{
    public readonly int MinIsoX;
    public readonly int MaxIsoX;

    public readonly int MinIsoY;
    public readonly int MaxIsoY;

    public IsometricDepthBounds(int minIsoX, int maxIsoX, int minIsoY, int maxIsoY)
    {
        MinIsoX = minIsoX;
        MaxIsoX = maxIsoX;
        MinIsoY = minIsoY;
        MaxIsoY = maxIsoY;
    }

    /// <summary>
    /// Converts a pixel-grid point into the two independent isometric floor
    /// axes. The result is scaled by 32 relative to tile coordinates, but that
    /// constant scale does not affect comparisons.
    /// </summary>
    public static Vector2Int GridToIsoAxes(Vector2Int gridPoint)
    {
        int isoX = gridPoint.x + 2 * gridPoint.y;
        int isoY = -gridPoint.x + 2 * gridPoint.y;

        return new Vector2Int(isoX, isoY);
    }
}
