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
    [SerializeField] private TileBase groundTile;
    public Dictionary<Vector2Int, bool> GroundTiles = new Dictionary<Vector2Int, bool>(); // <Coordinate, Occupation>

    void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        InitializeGroundTiles();

    }


    void Update()
    {
    }

    private void Initialize()
    {

    }

    public bool GetWorldPosition(Vector2Int gridPosition, out Vector2 worldPos)
    {
        Vector2 originaloffset = new Vector2(0, originYOffset * PixelToWorldSize);

        Vector2 buildingPos = (Vector2)gridPosition * PixelToWorldSize;

        worldPos = buildingPos + originaloffset;

        return GroundTiles.ContainsKey(gridPosition);
    }

    #region Tiles

    private void InitializeGroundTiles()
    {
        // 1. Get starting offset height.
        // 2. Get the width. (Players can unlock more width later in the game)
        // 3. Add all tiles into GroundTiles
        // 4. Samething for WallTiles

        GroundTiles = new Dictionary<Vector2Int, bool>();
        GridDimension = new Vector2Int(PlayerRoom.Instance.GetWidth(), PlayerRoom.Instance.GetHeight());
        Vector2Int pixelCount = GridDimension * PixelPerRoom;

        if (mainTilemap)
        {
            mainTilemap.ClearAllTiles();
            for (int i = 0; i < GridDimension.x; i++)
            {
                for (int j = 0; j < GridDimension.y; j++)
                {
                    InitializeTile(new Vector2Int(i, j));
                }
            }
        }

        //int halfX = pixelCount.x / 2;

        //// Bottom Half
        //for (int y = 0; y < pixelCount.y / 4; y++)
        //{
        //    int allowedHalfX = ((y + 1) * 4) / 2;
        //    for (int x = -allowedHalfX; x < allowedHalfX; x++)
        //    {
        //        Vector2Int coord = new Vector2Int(x, y);
        //        GroundTiles.Add(coord, false);
        //    }
        //}

        //// Top Half
        //int startY = pixelCount.y / 4;
        //for (int y = startY; y < pixelCount.y / 2; y++)
        //{
        //    int allowedHalfX = (startY * 4) / 2 - (Mathf.Abs(y - startY) * 2);
        //    for (int x = -allowedHalfX; x < allowedHalfX; x++)
        //    {
        //        Vector2Int coord = new Vector2Int(x, y);
        //        GroundTiles.Add(coord, false);
        //    }
        //}

        //for (int i = -halfX; i < halfX; i++)
        //{
        //    for (int j = 0; j < pixelCount.y / 2; j++)
        //    {
        //        Vector2Int coord = new Vector2Int(i, j);

        //        GroundTiles.Add(coord, false);
        //    }
        //}
    }

    /// <summary>
    /// Adds a tile worth of points into the ground dictionary.
    /// Provided tileposition is in relation to the 0, 0 tile.
    /// </summary>
    /// <param name="tilePosition"></param>
    public void InitializeTile(Vector2Int tilePosition)
    {
        // Place the tile at the grid position
        mainTilemap.SetTile((Vector3Int)tilePosition, groundTile);

        int worldX = (tilePosition.x - tilePosition.y) * (PixelPerRoom / 2);
        int worldY = (tilePosition.x + tilePosition.y) * (PixelPerRoom / 4);

        Vector2Int startPixel = new Vector2Int(worldX, worldY);

        Vector2Int pixelCount = new Vector2Int(PixelPerRoom, PixelPerRoom);

        // Bottom Half
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
            }
        }

    }

    #endregion tiles
}
