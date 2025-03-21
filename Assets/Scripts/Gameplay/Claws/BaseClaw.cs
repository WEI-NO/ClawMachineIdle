using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class BaseClaw : MonoBehaviour
{
    // Controls the magnitude of the x input (-1.0f - 1.0f by default)
    public const float MaxPositiveInput = 1.0f;
    public const float MaxNegativeInput = -1.0f;

    #region Base Class
    protected virtual void OnAwake() { }
    protected virtual void OnEnabled() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnDisabled() { }
    protected virtual void OnDestroyed() { }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        armController = GetComponentInChildren<ClawArmController>();
        OnAwake();
    }
    private void OnEnable()
    {
        OnEnabled();
    }
    private void Start()
    {
        OnStart();
    }
    private void Update()
    {
        xInputActive = false;
        XInputDissipate();
        OnUpdate();
    }
    private void FixedUpdate()
    {
        MovementUpdate();
        DangleUpdate();
        OnFixedUpdate();
    }
    private void OnDisable()
    {
        OnDisabled();
    }
    private void OnDestroy()
    {
        OnDestroyed();
    }
    #endregion base class

    [Header("Components")]
    public Rigidbody2D rb;

    [Header("Arm Properties")]
    public ClawArmController armController;
    public float armOpenStrength = 5.0f;
    public float armCloseStrength = 5.0f;

    [Header("Dangle Properties")]
    public float maxDangleAngle = 50.0f;
    public float dangleStrength = 10.0f;
    public float dangleDissipateStrength = 50.0f; // When x input is 0, it uses a different strength
    private float targetDangleAngle;

    [Header("Movements")]
    public Vector2 origin;
    public Vector2 xBoundary = new Vector2(-1, 1);
    public float xInput;
    public bool xInputActive;
    public float xInputDissipateRate = 2.0f;
    public float xInputSensitivity = 1.0f;
    public float moveSpeed = 5.0f;

    #region Input

    // == X Input ==
    // Desc:
    //          Called from other script to control the x input.
    // Params:
    //          right : if move right if not left
    //          magnitude : magnitude of the movement (for joystick control)
    public void XInput(bool right, float magnitude = 1)
    {
        xInputActive = true;
        xInput += magnitude * xInputSensitivity * Time.deltaTime * (right ? 1.0f : -1.0f);
        xInput = Mathf.Clamp(xInput, MaxNegativeInput, MaxPositiveInput);
    }

    // == X Input Dissipate ==
    // Desc:
    //          Makes sure XInput dissipates to 0
    private void XInputDissipate()
    {
        xInput = Mathf.MoveTowards(xInput, 0.0f, xInputDissipateRate * Time.deltaTime);
    }

    #endregion input

    #region Movement

    // == Movement Update ==
    // Desc:
    //          Updates the x movement based on xInput
    private void MovementUpdate()
    {
        // Calculate movement
        float xVelocity = xInput * moveSpeed * Time.fixedDeltaTime;

        // Apply movement
        rb.linearVelocityX = xVelocity;

        // Clamp position within boundaries
        Vector2 clampedPosition = rb.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, origin.x + xBoundary.x, origin.x + xBoundary.y);

        // Apply clamped position
        rb.position = clampedPosition;
    }

    // == Dangle Update ==
    // Desc:
    //          Simulates dangling effects
    private void DangleUpdate()
    {
        float moveProgress = Mathf.Abs(xInput) / MaxPositiveInput; // Division is neglectable if MaxInput is always 1.0f
        float direction = xInput < 0 ? -1.0f : xInput > 0 ? 1.0f : 0.0f; // input > 0 = 1.0f, input < 0 = -1.0f, otherwise = 0.0f

        float newTargetDangleAngle = moveProgress * maxDangleAngle * direction;
        if (!xInputActive) newTargetDangleAngle = 0.0f; // Force target angle to 0 if no input is held.

        float alpha = (xInputActive ? dangleStrength : dangleDissipateStrength) * Time.fixedDeltaTime;
        targetDangleAngle = Mathf.Lerp(targetDangleAngle, newTargetDangleAngle, alpha);

        transform.localEulerAngles = new Vector3(0, 0, targetDangleAngle) * -1.0f; // Multiply by -1.0f to reverse 
    }

    #endregion movement
}
