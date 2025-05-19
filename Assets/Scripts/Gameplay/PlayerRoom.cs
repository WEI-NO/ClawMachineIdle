using CustomLibrary.References;
using UnityEngine;

public class PlayerRoom : MonoBehaviour
{
    public static PlayerRoom Instance;

    [Header("Player Stats")]
    [SerializeField] private Vector2Int RoomDimension;

    private void Awake()
    {
        Initializer.SetInstance(this);
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
