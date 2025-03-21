using System.Collections;
using UnityEngine;
using CustomLibrary.Math.Vector;
using CustomLibrary.Math;

[RequireComponent (typeof(Rigidbody2D))]
public class BaseClaw : MonoBehaviour
{
    // Controls the magnitude of the x input (-1.0f - 1.0f by default)
    public const float MaxPositiveInput = 1.0f;
    public const float MaxNegativeInput = -1.0f;
    // Movement epsilon value
    public const float MovementEpsilon = 0.0001f;

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

    [Header("Claw Properties")]
    public Vector2 origin;
    public bool movementLocked 
    { 
        get { return movementLockCounter != 0; }
        private set { } 
    }
    private int movementLockCounter = 0;

    [Header("Arm Properties")]
    public ClawArmController armController;
    public float armOpenStrength = 5.0f;
    public float armCloseStrength = 5.0f;

    [Header("Dangle Properties")]
    public float maxDangleAngle = 50.0f;
    public float dangleStrength = 10.0f;
    public float dangleDissipateStrength = 50.0f; // When x input is 0, it uses a different strength
    private float targetDangleAngle;

    [Header("Input Properties")]
    public float xInput;
    public float xInputDissipateRate = 2.0f;
    public float xInputSensitivity = 1.0f;
    [Header("Developement View - Input")]
    public bool xInputActive;

    [Header("Movement Properties")]
    public Vector2 xBoundary = new Vector2(-1, 1);
    public float moveSpeed = 5.0f;
    private Vector2 lastPosition;
    [Header("Developement View - Movement")]
    public bool clawIsMoving = false;

    [Header("Grab Properties")]
    public float dropLength = 10.0f; // The length the claw drops
    public float dropDuration = 2.0f;
    public float dropRetreatDelay = 0.5f; // Delay in seconds which the claw retreat to original position (After armCloseDelay)
    public float armExpandPercentage = 0.2f; // At what point in Grab Sequence do the arm expand
    public float armCloseDelay = 0.8f; // Delay in seconds which the arm closes

    #region Input

    // == X Input ==
    // Desc:
    //          Called from other script to control the x input.
    // Params:
    //          right : if move right if not left
    //          magnitude : magnitude of the movement (for joystick control)
    public void XInput(bool right, float magnitude = 1)
    {
        // When movement is locked
        if (movementLocked)
        {
            xInputActive = false;
            xInput = 0;
            return;
        }

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

        clawIsMoving = (rb.position - lastPosition).sqrMagnitude > MovementEpsilon;
        lastPosition = rb.position;
    }

    // == Dangle Update ==
    // Desc:
    //          Simulates dangling effects
    private void DangleUpdate()
    {
        float moveProgress = Mathf.Abs(xInput) / MaxPositiveInput; // Division is neglectable if MaxInput is always 1.0f
        float direction = xInput < 0 ? -1.0f : xInput > 0 ? 1.0f : 0.0f; // input > 0 = 1.0f, input < 0 = -1.0f, otherwise = 0.0f

        float newTargetDangleAngle = moveProgress * maxDangleAngle * direction;
        if (!xInputActive || !clawIsMoving) 
            newTargetDangleAngle = 0.0f; // Force target angle to 0 if no input is held OR not moving

        float alpha = (xInputActive ? dangleStrength : dangleDissipateStrength) * Time.fixedDeltaTime;
        targetDangleAngle = Mathf.Lerp(targetDangleAngle, newTargetDangleAngle, alpha);

        transform.localEulerAngles = new Vector3(0, 0, targetDangleAngle) * -1.0f; // Multiply by -1.0f to reverse 
    }

    // == Start Grab Sequence ==
    // Desc:
    //          Calls the grab sequence coroutine to handle dropping sequence.
    public void StartGrabSequence()
    {
        if (movementLocked) return;

        StartCoroutine(GrabSequence());
    }

    private IEnumerator GrabSequence()
    {
        LockMovement(); // Lock Movement
        float elapsedTime = 0.0f;
        float targetYPosition = origin.y - dropLength;
        float startYPosition = transform.position.y;

        // Drop down
        while (elapsedTime < dropDuration)
        {
            float progress = elapsedTime / dropDuration;
            if (progress >= armExpandPercentage)
            {
                // Open Arm
                armController.SetTargetProgress(ArmState.Open, armOpenStrength);
            }
            float currentTargetYPosition = Mathf.SmoothStep(startYPosition, targetYPosition, progress);
            transform.position = Vector3Extension.ValueSwap(transform.position, iVector3.y, currentTargetYPosition);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(armCloseDelay);

        armController.SetTargetProgress(ArmState.Close, armCloseStrength);

        yield return new WaitForSeconds(dropRetreatDelay);
        
        elapsedTime = 0.0f;
        targetYPosition = origin.y;
        startYPosition = transform.position.y;


        // Drop down
        while (elapsedTime < dropDuration)
        {
            float progress = elapsedTime / dropDuration;
            float currentTargetYPosition = Mathf.SmoothStep(startYPosition, targetYPosition, progress);
            transform.position = Vector3Extension.ValueSwap(transform.position, iVector3.y, currentTargetYPosition);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        UnlockMovement();
    }

    #endregion movement

    #region State

    // == Lock Movement ==
    // Desc:
    //          Locks the movement
    public void LockMovement()
    {
        movementLockCounter++;
    }

    // == Unlock Movement ==
    // Desc:
    //          Unlocks the movement
    public void UnlockMovement()
    {
        movementLockCounter--;
        if (movementLockCounter < 0)
        {
            Debug.LogWarning($"{gameObject.name}: Movement Lock Counter is below 0 ({movementLockCounter}).");
            movementLockCounter = 0;
        }
    }

    #endregion state
}
