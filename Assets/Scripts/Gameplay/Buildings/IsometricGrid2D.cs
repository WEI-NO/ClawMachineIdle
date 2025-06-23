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
    private const int PixelPerRoom = 32;
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
    public Dictionary<Vector2Int, bool> GroundTiles = new Dictionary<Vector2Int, bool>(); // <Coordinate, Occupation>
    public Dictionary<Vector2Int, bool> WallTiles = new Dictionary<Vector2Int, bool>(); // <Coordinate, Occupation>

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


    void Update()
    {
    }

    private void Initialize()
    {

    }

    public bool GetWorldPosition(Vector2Int gridPosition, out Vector2 worldPos, bool forWallObject = false)
    {

        Vector2 originaloffset = new Vector2(0, originYOffset * PixelToWorldSize);

        Vector2 buildingPos = (Vector2)gridPosition * PixelToWorldSize;

        worldPos = buildingPos + originaloffset;
        if (!forWallObject)
        {
            return GroundTiles.ContainsKey(gridPosition);
        }
        else
        {
            return WallTiles.ContainsKey(gridPosition);
        }
    }

    #region Tiles

    private void InitializeWallTiles()
    {
        WallTiles = new Dictionary<Vector2Int, bool>();
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
        int width = 16;
        int height = 95;
        if (left)
        {
            int xDiff = Mathf.Abs(tilePosition.x - GridDimension.x);
            var origin = new Vector2Int(0, tilePosition.x * 16);
            origin += new Vector2Int((xDiff * -16), (xDiff * 8) + 1);

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
                }
            }
        }
        // Right
        else
        {
            int xDiff = Mathf.Abs(tilePosition.y - GridDimension.y);
            var origin = new Vector2Int(0, tilePosition.y * 16);
            origin += new Vector2Int((xDiff * 16), (xDiff * 8) + 1);


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
                //debugPrint += $"{coord} | ";
            }
        }

        //debugPrint += "]";
        //print(debugPrint);
    }

    #endregion tiles
}
