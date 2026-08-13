using CustomLibrary.References;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Implementation of an isometric grid.
/// Free formed grid.
/// </summary>
public class IsometricGrid2D : MonoBehaviour
{
    public static IsometricGrid2D Instance;

    private const float PixelToWorldSize = 0.03125f;

    // Ground tile geometry, expressed in source-art pixels (9.2 - named geometry values).
    private const int GroundTilePixelWidth = 32;
    private const int GroundTilePixelHeight = 16;
    private const int HalfTilePixelWidth = 16;
    private const int HalfTilePixelHeight = 8;
    private const int PixelPerRoom = GroundTilePixelWidth;

    [Header("Grid Properties")]
    public int alignment = 1; // Determines how many pixels is moved per tile.
    public int originYOffset = 9; // In Pixel
    public Vector2Int GridDimension;

    [Header("Ground Tile")]
    [SerializeField] private Tilemap mainTilemap; // For alignment purposes
    [SerializeField] private Tilemap wallTilemap; // For alignment purposes
    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase wallTile_x;
    [SerializeField] private TileBase wallTile_y;
    [SerializeField] private TileBase wallCornerTile;
    [SerializeField] private int wallPixelHeight = 95;
    public Dictionary<Vector2Int, bool> GroundTiles = new Dictionary<Vector2Int, bool>(); // <Coordinate, Occupation>
    public Dictionary<Vector2Int, bool> WallTiles = new Dictionary<Vector2Int, bool>(); // <Coordinate, Occupation>

    private readonly HashSet<Vector2Int> validGroundCells = new();
    private readonly Dictionary<Vector2Int, IsometricBuilding> groundOccupants = new();

    private readonly HashSet<Vector2Int> validWallCells = new();
    private readonly Dictionary<Vector2Int, IsometricBuilding> wallOccupants = new();

    void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        // 1. Initialize Ground Tiles then Wall Tiles
        InitializeGroundTiles();
        InitializeWallTiles();

    }

    /// <summary>
    /// Converts a pixel-grid coordinate into a world position.
    /// Returns false when the coordinate is not a valid cell, in which case
    /// callers must not apply <paramref name="worldPos"/> to a transform (4.4).
    /// </summary>
    public bool TryGetWorldPosition(Vector2Int gridPosition, out Vector2 worldPos, bool forWallObject = false)
    {
        Vector2 originaloffset = new Vector2(0, originYOffset * PixelToWorldSize);

        Vector2 buildingPos = (Vector2)gridPosition * PixelToWorldSize;

        worldPos = buildingPos + originaloffset;

        return forWallObject
            ? validWallCells.Contains(gridPosition)
            : validGroundCells.Contains(gridPosition);
    }

    // Backwards-compatible alias for the pre-refactor name.
    public bool GetWorldPosition(Vector2Int gridPosition, out Vector2 worldPos, bool forWallObject = false)
        => TryGetWorldPosition(gridPosition, out worldPos, forWallObject);

    #region Valid Cell Queries (7.4)

    public bool IsValidGroundCell(Vector2Int coordinate) => validGroundCells.Contains(coordinate);

    public bool IsValidWallCell(Vector2Int coordinate) => validWallCells.Contains(coordinate);

    public IReadOnlyCollection<Vector2Int> ValidGroundCells => validGroundCells;
    public IReadOnlyCollection<Vector2Int> ValidWallCells => validWallCells;

    /// <summary>
    /// Snaps a pixel-grid coordinate to the configured <see cref="alignment"/> step (8.4).
    /// </summary>
    public Vector2Int SnapToAlignment(Vector2Int position)
    {
        int step = Mathf.Max(1, alignment);

        return new Vector2Int(
            Mathf.RoundToInt(position.x / (float)step) * step,
            Mathf.RoundToInt(position.y / (float)step) * step);
    }

    #endregion

    #region Tiles

    private void InitializeWallTiles()
    {
        WallTiles = new Dictionary<Vector2Int, bool>();
        validWallCells.Clear();
        wallOccupants.Clear();
        var origin = GridDimension;

        if (wallTilemap)
        {
            wallTilemap.ClearAllTiles();
            wallTilemap.SetTile((Vector3Int)origin, wallCornerTile);
            for (int i = 1; i <= GridDimension.x; i++)
            {
                var newCoord = origin - new Vector2Int(i, 0);
                InitializeWallTile(newCoord, true);
            }

            for (int i = 1; i <= GridDimension.y; i++)
            {
                var newCoord = origin - new Vector2Int(0, i);
                InitializeWallTile(newCoord, false);
            }
        }
    }

    private void InitializeGroundTiles()
    {
        // 1. Get starting offset height.
        // 2. Get the width. (Players can unlock more width later in the game)
        // 3. Add all tiles into GroundTiles
        // 4. Samething for WallTiles

        GroundTiles = new Dictionary<Vector2Int, bool>();
        validGroundCells.Clear();
        groundOccupants.Clear();
        GridDimension = new Vector2Int(PlayerRoom.Instance.GetWidth(), PlayerRoom.Instance.GetHeight());

        if (mainTilemap)
        {
            mainTilemap.ClearAllTiles();
            for (int i = 0; i < GridDimension.x; i++)
            {
                for (int j = 0; j < GridDimension.y; j++)
                {
                    InitializeGroundTile(new Vector2Int(i, j));
                }
            }
        }
    }

    public void InitializeWallTile(Vector2Int tilePosition, bool left)
    {
        wallTilemap.SetTile((Vector3Int)tilePosition, left ? wallTile_x : wallTile_y);
        int width = HalfTilePixelWidth;
        int height = wallPixelHeight;
        if (left)
        {
            int xDiff = Mathf.Abs(tilePosition.x - GridDimension.x);
            var origin = new Vector2Int(0, tilePosition.x * HalfTilePixelWidth);
            origin += new Vector2Int((xDiff * -HalfTilePixelWidth), (xDiff * HalfTilePixelHeight) + 1);

            for (int i = origin.x; i < origin.x + width; i++)
            {
                int currentHeight = Mathf.FloorToInt((i - origin.x) / 2.0f);
                int currentY = origin.y + currentHeight;
                for (int j = currentY; j < currentY + height; j++)
                {
                    var coord = new Vector2Int(i, j);
                    if (WallTiles.ContainsKey(coord))
                    {
                        continue;
                    }
                    WallTiles.Add(coord, false);
                    validWallCells.Add(coord);
                }
            }
        }
        // Right
        else
        {
            int xDiff = Mathf.Abs(tilePosition.y - GridDimension.y);
            var origin = new Vector2Int(0, tilePosition.y * HalfTilePixelWidth);
            origin += new Vector2Int((xDiff * HalfTilePixelWidth), (xDiff * HalfTilePixelHeight) + 1);


            for (int i = origin.x - 1 ; i >= origin.x - width; i--)
            {
                int currentHeight = Mathf.FloorToInt(((origin.x - 1) - i) / 2.0f);
                int currentY = origin.y + currentHeight;
                for (int j = currentY; j < currentY + height; j++)
                {
                    var coord = new Vector2Int(i, j);
                    if (WallTiles.ContainsKey(coord))
                    {
                        continue;
                    }
                    WallTiles.Add(coord, false);
                    validWallCells.Add(coord);
                }
            }
        }

    }

    /// <summary>
    /// Adds a tile worth of points into the ground dictionary.
    /// Provided tileposition is in relation to the 0, 0 tile.
    /// </summary>
    /// <param name="tilePosition"></param>
    public void InitializeGroundTile(Vector2Int tilePosition)
    {
        // Place the tile at the grid position
        mainTilemap.SetTile((Vector3Int)tilePosition, groundTile);

        int worldX = (tilePosition.x - tilePosition.y) * (PixelPerRoom / 2);
        int worldY = (tilePosition.x + tilePosition.y) * (PixelPerRoom / 4);

        Vector2Int startPixel = new Vector2Int(worldX, worldY);

        Vector2Int pixelCount = new Vector2Int(PixelPerRoom, PixelPerRoom);
        // Bottom Half
        //string debugPrint = $"{tilePosition} => [ ";
        for (int y = 0; y < pixelCount.y / 4; y++)
        {
            int allowedHalfX = ((y + 1) * 4) / 2;
            for (int x = -allowedHalfX; x < allowedHalfX; x++)
            {
                Vector2Int coord = startPixel + new Vector2Int(x, y);
                if (GroundTiles.ContainsKey(coord))
                {
                    continue;
                }
                GroundTiles.Add(coord, false);
                validGroundCells.Add(coord);
                //debugPrint += $"{coord} | ";
            }
        }

        // Top Half
        int startY = pixelCount.y / 4;
        for (int y = startY; y < pixelCount.y / 2; y++)
        {
            int allowedHalfX = (startY * 4) / 2 - (Mathf.Abs(y - startY) * 2);
            for (int x = -allowedHalfX; x < allowedHalfX; x++)
            {
                Vector2Int coord = startPixel + new Vector2Int(x, y);
                if (GroundTiles.ContainsKey(coord))
                {
                    continue;
                }
                GroundTiles.Add(coord, false);
                validGroundCells.Add(coord);
                //debugPrint += $"{coord} | ";
            }
        }
    }

    #endregion tiles

    #region Grid Operations

    // Snap candidates tried, in order, when the requested position is blocked (5.3).
    private static readonly Vector2Int[] SnapOffsets =
    {
        Vector2Int.zero,
        new Vector2Int(2, 0),
        new Vector2Int(-2, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    // Reused across placement queries to avoid iterating a mutating dictionary.
    private readonly List<Vector2Int> reusableCellBuffer = new();

    /// <summary>
    /// Returns true when every cell the building would occupy at
    /// <paramref name="position"/> is a valid cell and is not already reserved
    /// by a different building. The building may overlap its own reservation (5.1).
    /// </summary>
    public bool CanPlace(
        IsometricBuilding building,
        Vector2Int position,
        Orientation orientation)
    {
        if (building == null)
        {
            return false;
        }

        bool wall = building.blueprint.IsWallObject;
        HashSet<Vector2Int> valid = wall ? validWallCells : validGroundCells;
        Dictionary<Vector2Int, IsometricBuilding> occupants = wall ? wallOccupants : groundOccupants;

        // Boundary check on the outline (matches the movement validation).
        foreach (Vector2Int cell in building.GetOccupiedCells(position, orientation))
        {
            if (!valid.Contains(cell))
            {
                return false;
            }
        }

        // Overlap check on the filled footprint, so a building placed fully
        // inside another is detected, not only edge overlaps.
        foreach (Vector2Int cell in building.GetFilledCells(position, orientation))
        {
            if (occupants.TryGetValue(cell, out IsometricBuilding occupant) && occupant != building)
            {
                return false;
            }
        }

        return true;
    }

    public bool TryPlace(
        IsometricBuilding building,
        Vector2Int position,
        Orientation orientation)
    {
        if (!CanPlace(building, position, orientation))
        {
            return false;
        }

        // Release any prior reservation, then reserve the new footprint.
        Remove(building);
        Reserve(building, position, orientation);
        return true;
    }

    public bool TryMove(
        IsometricBuilding building,
        Vector2Int targetPosition)
    {
        if (building == null)
        {
            return false;
        }

        return TryPlace(building, targetPosition, building.blueprint.currentOrientation);
    }

    /// <summary>
    /// Searches <see cref="SnapOffsets"/> for the first placeable position near
    /// <paramref name="requestedPosition"/> (5.3). Returns the building's current
    /// grid position when nothing valid is found.
    /// </summary>
    public bool TryFindNearestValidPosition(
        IsometricBuilding building,
        Vector2Int requestedPosition,
        Orientation orientation,
        out Vector2Int result)
    {
        if (building != null)
        {
            foreach (Vector2Int offset in SnapOffsets)
            {
                Vector2Int candidate = requestedPosition + offset;

                if (CanPlace(building, candidate, orientation))
                {
                    result = candidate;
                    return true;
                }
            }

            result = building.blueprint.GridPosition;
        }
        else
        {
            result = requestedPosition;
        }

        return false;
    }

    /// <summary>
    /// Releases every cell reserved by <paramref name="building"/>. Safe to call
    /// more than once (11.3).
    /// </summary>
    public void Remove(IsometricBuilding building)
    {
        if (building == null)
        {
            return;
        }

        RemoveFrom(groundOccupants, building);
        RemoveFrom(wallOccupants, building);
    }

    private void Reserve(IsometricBuilding building, Vector2Int position, Orientation orientation)
    {
        Dictionary<Vector2Int, IsometricBuilding> occupants =
            building.blueprint.IsWallObject ? wallOccupants : groundOccupants;

        // Reserve the filled footprint so overlaps against the interior register.
        foreach (Vector2Int cell in building.GetFilledCells(position, orientation))
        {
            occupants[cell] = building;
        }
    }

    private void RemoveFrom(
        Dictionary<Vector2Int, IsometricBuilding> occupants,
        IsometricBuilding building)
    {
        reusableCellBuffer.Clear();

        foreach (KeyValuePair<Vector2Int, IsometricBuilding> entry in occupants)
        {
            if (entry.Value == building)
            {
                reusableCellBuffer.Add(entry.Key);
            }
        }

        foreach (Vector2Int cell in reusableCellBuffer)
        {
            occupants.Remove(cell);
        }
    }

    #endregion
}
