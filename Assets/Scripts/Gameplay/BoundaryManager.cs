using UnityEngine;
using UnityEngine.UI;

public class BoundaryManager : MonoBehaviour
{
    [Header("Table Properties")]
    public Transform table;
    public float tableScaleMultiplier = 1.0f;
    [Range(0f, 1f)]
    public float tableVerticalAnchor = 0.0f; // 0 = bottom, 1 = top of screen
    private float lastVerticalAnchor;

    [Header("Boundary Properties")]
    public Transform leftWall;
    public Transform rightWall;
    public float wallThickness = 1f;
    public Vector2 lastScreenResolution = Vector2.zero;

    [Header("Buttons Properties")]
    public float buttonSizeRatio;
    public float joystickSizeRatio;
    [SerializeField] private RefreshButton refreshButton;
    [SerializeField] private GrabButton grabButton;
    [SerializeField] private JoystickTouchController joystickController;

    void Start()
    {
        ScaleBoundaryToScreen();
    }

    private void Update()
    {
        if (lastVerticalAnchor != tableVerticalAnchor)
        {
            ScaleBoundaryToScreen();
        }

        Camera cam = Camera.main;
        float screenHeight = cam.orthographicSize * 2.0f;
        float screenWidth = screenHeight * cam.aspect;
        if (screenHeight != lastScreenResolution.y || screenWidth != lastScreenResolution.x)
        {
            ScaleBoundaryToScreen();
        }
    }

    void ScaleBoundaryToScreen()
    {
        Camera cam = Camera.main;
        float screenHeight = cam.orthographicSize * 2.0f;
        float screenWidth = screenHeight * cam.aspect;

        lastScreenResolution = new Vector2(screenWidth, screenHeight);

        // ---------- Table Scaling ----------
        float newScaleX = screenWidth / 2.0f;
        table.localScale = new Vector3(newScaleX, newScaleX, 1f) * tableScaleMultiplier;

        SpriteRenderer sr = table.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Bounds tableBounds = sr.bounds;
        float tableHeight = tableBounds.size.y;

        float topY = cam.orthographicSize;
        float bottomY = -cam.orthographicSize;
        float topAnchorY = Mathf.Lerp(bottomY, topY, tableVerticalAnchor);
        float tableCenterY = topAnchorY - (tableHeight / 2f);

        table.position = new Vector3(0f, tableCenterY, table.position.z);
        lastVerticalAnchor = tableVerticalAnchor;

        // ---------- Walls Scaling and Positioning ----------

        //// Left Wall
        //leftWall.position = new Vector2(-screenWidth / 2 + wallThickness / 2, 0);
        //leftWall.localScale = new Vector3(wallThickness, screenHeight);

        //// Right Wall
        //rightWall.position = new Vector2(screenWidth / 2 - wallThickness / 2, 0);
        //rightWall.localScale = new Vector3(wallThickness, screenHeight, 1);
    }
}