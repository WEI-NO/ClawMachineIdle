using UnityEngine;

public class RoomCamera : MonoBehaviour
{
    public static RoomCamera Instance;
    [Header("Components")]
    public Camera cam;

    [Header("Lerp Settings")]
    public float lerpSpeed = 5f; // Adjust for desired smoothness

    private Vector3 targetPosition;

    // Drag state
    private Vector2 lastScreenPosition;
    private bool isDragging = false;
    [SerializeField] private float cameraPanSensitivity = 1.0f;

    private void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        targetPosition = transform.position;
    }

    private void Update()
    {
        // Smoothly move camera towards targetPosition
        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
    }

    // Call this on touch down or drag start
    public void OnDragStart(Vector2 screenPosition)
    {
        isDragging = true;
        lastScreenPosition = screenPosition;
    }

    // Call this every frame on drag move
    public void OnDragUpdate(Vector2 screenPosition)
    {
        if (!isDragging) return;
        // Convert screen to world for both positions
        Vector2 worldCurrent = cam.ScreenToWorldPoint(screenPosition);
        Vector2 worldLast = cam.ScreenToWorldPoint(lastScreenPosition);
        Vector2 delta = worldCurrent - worldLast;
        // Move targetPosition (invert direction for natural drag)
        targetPosition -= (Vector3)(delta * cameraPanSensitivity);
        lastScreenPosition = screenPosition;
    }

    // Call this when drag ends or touch lifts
    public void OnDragEnd()
    {
        isDragging = false;
    }
}
