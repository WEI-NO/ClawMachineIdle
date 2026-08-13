using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickTouchController : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler
{
    public Action<float> OnJoystickProgressUpdate;

    [Header("Touch Properties")]
    public bool isTouched;

    [Header("Control Properties")]
    [SerializeField] private float maxRotateAngle = 45f;

    [Tooltip("How much of the screen width must be dragged to move through the full joystick range.")]
    [SerializeField] private float dragScreenPercentage = 0.25f;

    [Tooltip("Degrees per second while returning to the center.")]
    [SerializeField] private float resetSpeed = 180f;

    [SerializeField] private Transform target;

    private Animator animator;

    [Header("Animator Parameters")]
    [SerializeField] private string downTrigger = "Pressed";
    [SerializeField] private string startDownTrigger = "Down";

    private void Awake()
    {
        if (target == null)
            target = transform;

        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!isTouched)
            ResetPosition();

        SendJoystickInput();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Screen.width <= 0)
            return;

        float currentAngle = GetSignedZAngle();

        // Convert the pointer movement into a percentage of the screen width.
        float requiredDragDistance =
            Mathf.Max(1f, Screen.width * dragScreenPercentage);

        float angleDelta =
            eventData.delta.x / requiredDragDistance *
            maxRotateAngle * 2f;

        currentAngle -= angleDelta;
        currentAngle = Mathf.Clamp(
            currentAngle,
            -maxRotateAngle,
            maxRotateAngle
        );

        SetZAngle(currentAngle);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isTouched = true;

        if (animator != null)
        {
            animator.SetBool(downTrigger, true);
            animator.SetTrigger(startDownTrigger);
        }

        if (TopHUDAlphaController.Instance)
        {
            TopHUDAlphaController.Instance.DisableAlpha();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouched = false;

        if (animator != null)
            animator.SetBool(downTrigger, false);

        if (TopHUDAlphaController.Instance)
        {
            TopHUDAlphaController.Instance.EnableAlpha();
        }
    }

    private void ResetPosition()
    {
        float currentAngle = GetSignedZAngle();

        currentAngle = Mathf.MoveTowards(
            currentAngle,
            0f,
            resetSpeed * Time.deltaTime
        );

        SetZAngle(currentAngle);
    }

    private void SendJoystickInput()
    {
        float progress = Mathf.Clamp(
            GetSignedZAngle() / maxRotateAngle,
            -1f,
            1f
        );

        OnJoystickProgressUpdate?.Invoke(progress);

        if (CraneStickController.Instance != null)
            CraneStickController.Instance.SimulateXInput(-progress);
    }

    private float GetSignedZAngle()
    {
        return Mathf.DeltaAngle(0f, target.eulerAngles.z);
    }

    private void SetZAngle(float angle)
    {
        Vector3 eulerAngles = target.eulerAngles;
        eulerAngles.z = angle;
        target.eulerAngles = eulerAngles;
    }
}