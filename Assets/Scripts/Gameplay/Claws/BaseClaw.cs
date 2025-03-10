using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class BaseClaw : MonoBehaviour
{
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

    [Header("Claw Properties")]
    private float rotationVelocity = 0f;
    public float rotationStrength = 100.0f;
    public float springStrength = 5f;  // Higher = more responsive
    public float damping = 2f;            // Higher = less wobble
    public Vector3 lastPosition;

    [Header("Movements")]
    public Vector2 origin;
    public Vector2 xBoundary = new Vector2(-1, 1);
    public float xInput;
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
        xInput += magnitude * xInputSensitivity * Time.deltaTime * (right ? 1.0f : -1.0f);
        xInput = Mathf.Clamp(xInput, -1.0f, 1.0f);
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
        // Convert current Z rotation to a signed angle (-180 to 180)
        float currentAngle = transform.eulerAngles.z;
        if (currentAngle > 180f) currentAngle -= 360f;  // Convert to -180 to 180 range

        // Calculate target angle based on movement
        float targetAngle = Mathf.Clamp((transform.position.x - lastPosition.x) * (-1.0f * rotationStrength), -90f, 90f);
        // Apply spring force
        float force = (targetAngle - currentAngle) * springStrength;
        rotationVelocity += force * Time.fixedDeltaTime;

        // Apply damping to prevent infinite wobbling
        rotationVelocity *= Mathf.Exp(-damping * Time.fixedDeltaTime);

        // Apply the new rotation
        float newAngle = currentAngle + rotationVelocity;
        transform.rotation = Quaternion.Euler(0, 0, newAngle);

        // Store the last position for velocity calculations
        lastPosition = transform.position;
    }

    #endregion movement
}
