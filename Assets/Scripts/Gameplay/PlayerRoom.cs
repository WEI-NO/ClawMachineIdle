using CustomLibrary.References;
using UnityEngine;

public class PlayerRoom : MonoBehaviour
{
    public static PlayerRoom Instance;

    [Header("Player Stats")]
    [SerializeField] private Vector2Int DefaultDimension;
    [SerializeField] private Vector2Int RoomDimension;

    [Header("Debug Buttons")]
    public bool ExpandXDirection = false;
    public bool ExpandYDirection = false;

    private void Awake()
    {
        Initializer.SetInstance(this);

        RoomDimension = DefaultDimension;
    }

    private void Update()
    {
        if (ExpandXDirection)
        {
            ExpandX();
            ExpandXDirection = false;
        } else if (ExpandYDirection)
        {
            ExpandY();
            ExpandYDirection = false;
        }
    }

    public void ExpandX()
    {
        //InitializeTile(new Vector2Int(-1, 0));
        //InitializeTile(new Vector2Int(-1, 1));
        //InitializeTile(new Vector2Int(-1, 2));
        int nextXLayer = DefaultDimension.x - RoomDimension.x - 1;
        int startY = DefaultDimension.y - RoomDimension.y;
        for (int i = startY; i < DefaultDimension.y; i++)
        {
            IsometricGrid2D.Instance.InitializeGroundTile(new Vector2Int(nextXLayer, i));
        }

        IsometricGrid2D.Instance.InitializeWallTile(new Vector2Int(nextXLayer, DefaultDimension.y), true);
        RoomDimension.x += 1;
    }

    public void ExpandY()
    {
        int nextYLayer = DefaultDimension.y - RoomDimension.y - 1;
        int startX = DefaultDimension.x - RoomDimension.x;
        for (int i = startX; i < DefaultDimension.x; i++)
        {
            IsometricGrid2D.Instance.InitializeGroundTile(new Vector2Int(i, nextYLayer));
        }
        IsometricGrid2D.Instance.InitializeWallTile(new Vector2Int(DefaultDimension.x, nextYLayer), false);
        RoomDimension.y += 1;
    }

    public int GetWidth()
    {
        return RoomDimension.x;
    }

    public int GetHeight()
    {
        return RoomDimension.y;
    }
}
