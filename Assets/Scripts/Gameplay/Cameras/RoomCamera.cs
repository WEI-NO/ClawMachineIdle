using CustomLibrary.References;
using UnityEngine;

public class RoomCamera : MonoBehaviour
{
    public static RoomCamera Instance;
    [Header("Components")]
    public Camera cam;

    [Header("Lerp Settings")]
    public float lerpSpeed = 5f; // Adjust for desired smoothness

    private Vector3 targetPosition;

    private void Awake()
    {
        Initializer.SetInstance(this);
        cam = GetComponent<Camera>();
        targetPosition = transform.position;
    }

    /// <summary>
    /// Call this to set a new camera destination (world space).
    /// </summary>
    public void SetTargetPosition(Vector3 position)
    {
        // Maintain Z position for orthographic camera
        position.z = transform.position.z;
        targetPosition = position;
        transform.position = targetPosition;
    }

    private void Update()
    {
        // Smoothly move camera towards targetPosition
        //transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
    }
}
