using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickTouchController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public Action<float> OnJoystickProgressUpdate;

    [Header("Touch Properties")]
    public bool isTouched;
    [Header("Control Properties")]
    public float maxRotateAngle = 45f;
    public float rotateStrength = 10.0f;
    public Vector2 lastMousePosition;
    public float resetStrength = 1.0f;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 latestTouchPos = eventData.position;
        float xDiff = latestTouchPos.x - lastMousePosition.x;

        Vector3 currAngle = transform.eulerAngles;

        // Convert from 0–360 to -180–180
        float zAngle = currAngle.z;
        if (zAngle > 180f) zAngle -= 360f;

        // Rotate based on drag amount
        zAngle -= rotateStrength * xDiff * Time.deltaTime;

        // Clamp within desired range
        zAngle = Mathf.Clamp(zAngle, -maxRotateAngle, maxRotateAngle);

        // Convert back to 0–360 before applying
        if (zAngle < 0f) zAngle += 360f;

        currAngle.z = zAngle;
        transform.eulerAngles = currAngle;

        lastMousePosition = latestTouchPos;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        lastMousePosition = eventData.position;
        isTouched = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouched = false;
    }

    void FixedUpdate()
    {
        float zAngle = transform.eulerAngles.z;

        // Normalize to [-180, 180]
        if (zAngle > 180f)
            zAngle -= 360f;

        // Calculate progress in range [-1, 1]
        float progress = Mathf.Clamp(zAngle / maxRotateAngle, -1f, 1f);
        OnJoystickProgressUpdate?.Invoke(progress);

        ResetPositionUpdate();
    }

    private void ResetPositionUpdate()
    {
        if (isTouched) return;

        Vector3 currAngle = transform.eulerAngles;

        // Smoothly move Z angle toward 0 using ResetStrength (degrees per second)
        currAngle.z = Mathf.LerpAngle(currAngle.z, 0f, resetStrength * Time.deltaTime);

        if (currAngle.z < 0.01f)
        {
            currAngle.z = 0f;
        }

        transform.eulerAngles = currAngle;
    }
}
