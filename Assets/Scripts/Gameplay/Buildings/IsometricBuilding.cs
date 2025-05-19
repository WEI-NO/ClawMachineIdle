using UnityEngine;

public class IsometricBuilding : MonoBehaviour
{
    [Header("Building Properties")]
    private Vector2Int lastGridPosition;
    public Vector2Int GridPosition;
    public Vector2Int PixelDimension; // Dimension in pixel.

    private void Update()
    {
        DEBUG_TestIsometricGridInput();
        if (IsometricGrid2D.Instance.GetWorldPosition(GridPosition, out Vector2 worldPos))
        {
            transform.position = worldPos;
        } else
        {
            GridPosition = lastGridPosition;
        }
    }

    private void DEBUG_TestIsometricGridInput()
    {
        lastGridPosition = GridPosition;
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            GridPosition.y++;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            GridPosition.y--;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            GridPosition.x--;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            GridPosition.x++;
        }
    }
}
