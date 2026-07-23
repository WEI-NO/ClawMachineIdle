using CustomLibrary.References;
using UnityEngine;

public class CraneStickController : MonoBehaviour
{
    public static CraneStickController Instance;

    [Header("References")]
    public Rigidbody2D rb;
    public ClawObject claw;

    [Header("State")]
    public bool isActive;
    public bool isMoving;

    [Header("Horizontal Movement")]
    public float horizontalMoveSpeed = 5f;

    [Range(0f, 1f)]
    public float xInput;

    public float xVelocity;
    public Vector2 xLimits;

    [Header("Target Height")]
    public float targetY;
    public Vector2 yLimits = new Vector2(-5f, 5f);
    public float grabY;
    public float idleY;
    public float inactiveY;

    [Header("Vertical Movement")]
    public float verticalSpeed = 5f;
    public float arriveThreshold = 0.05f;
    public float activationSpeed = 5f;

    private void Awake()
    {
        Initializer.SetInstance(this);

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        isActive = true;
    }

    private void Start()
    {
        SetTargetY(idleY);
        CalculateHorizontalLimits();
    }

    private void Update()
    {
        HandleDebugInput();
        UpdateVerticalConstraint();
    }

    private void FixedUpdate()
    {
        CraneMovementUpdate();
        VerticalMovementUpdate();
        ClampXPosition();
    }

    #region Input

    public void SimulateXInput(float input)
    {
        // Input changes immediately. No interpolation is applied here.
        xInput = Mathf.Clamp(input, -1f, 1f);
    }

    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.Period))
        {
            SetActive(!isActive);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            SetTargetY(targetY - 10f * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            SetTargetY(targetY + 10f * Time.deltaTime);
        }
    }

    #endregion

    #region Horizontal Movement

    public void CraneMovementUpdate()
    {
        if (rb == null)
            return;

        if (claw != null && claw.inSequence)
        {
            StopHorizontalMovement();
            return;
        }

        float desiredVelocity = xInput * horizontalMoveSpeed;

        float predictedX =
            rb.position.x +
            desiredVelocity * Time.fixedDeltaTime;

        bool wouldCrossLeftLimit =
            desiredVelocity < 0f &&
            predictedX < xLimits.x;

        bool wouldCrossRightLimit =
            desiredVelocity > 0f &&
            predictedX > xLimits.y;

        if (wouldCrossLeftLimit || wouldCrossRightLimit)
        {
            // Place the crane directly on the appropriate boundary instead
            // of allowing it to move outside and clamping it afterward.
            float clampedX = Mathf.Clamp(
                predictedX,
                xLimits.x,
                xLimits.y
            );

            rb.position = new Vector2(
                clampedX,
                rb.position.y
            );

            StopHorizontalMovement();
            return;
        }

        // Reversing away from a boundary reaches this immediately.
        xVelocity = desiredVelocity;
        rb.linearVelocityX = xVelocity;
    }

    private void StopHorizontalMovement()
    {
        xVelocity = 0f;

        if (rb != null)
            rb.linearVelocityX = 0f;
    }

    private void ClampXPosition()
    {
        if (rb == null)
            return;

        float clampedX = Mathf.Clamp(
            rb.position.x,
            xLimits.x,
            xLimits.y
        );

        bool wasOutsideLimits =
            !Mathf.Approximately(clampedX, rb.position.x);

        if (!wasOutsideLimits)
            return;

        rb.position = new Vector2(
            clampedX,
            rb.position.y
        );

        StopHorizontalMovement();
    }

    private void CalculateHorizontalLimits()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogWarning(
                "CraneStickController could not find the main camera.",
                this
            );

            return;
        }

        float screenHalfWidth =
            cam.orthographicSize * cam.aspect;

        float halfCraneWidth = 0.5f;

        SpriteRenderer spriteRenderer =
            GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            halfCraneWidth =
                spriteRenderer.bounds.extents.x;
        }

        xLimits = new Vector2(
            -screenHalfWidth + halfCraneWidth,
            screenHalfWidth - halfCraneWidth
        );
    }

    #endregion

    #region Vertical Movement

    public void VerticalMovementUpdate()
    {
        if (!isMoving || rb == null)
            return;

        Vector2 position = rb.position;
        float distance = targetY - position.y;

        if (Mathf.Abs(distance) <= arriveThreshold)
        {
            rb.linearVelocity = Vector2.zero;

            rb.MovePosition(
                new Vector2(position.x, targetY)
            );

            isMoving = false;
            return;
        }

        float direction = Mathf.Sign(distance);

        float newY =
            position.y +
            direction * verticalSpeed *
            Time.fixedDeltaTime;

        bool passedTargetMovingDown =
            direction < 0f &&
            newY <= targetY;

        bool passedTargetMovingUp =
            direction > 0f &&
            newY >= targetY;

        if (passedTargetMovingDown || passedTargetMovingUp)
        {
            newY = targetY;
        }

        newY = Mathf.Clamp(
            newY,
            yLimits.x,
            yLimits.y
        );

        rb.MovePosition(
            new Vector2(position.x, newY)
        );
    }

    private void UpdateVerticalConstraint()
    {
        if (rb == null)
            return;

        if (isMoving)
        {
            rb.constraints &=
                ~RigidbodyConstraints2D.FreezePositionY;
        }
        else
        {
            rb.constraints |=
                RigidbodyConstraints2D.FreezePositionY;
        }
    }

    public void SetVerticalSpeed(float speed)
    {
        verticalSpeed = speed;
    }

    public void SetTargetY(float newY)
    {
        targetY = Mathf.Clamp(
            newY,
            yLimits.x,
            yLimits.y
        );

        isMoving = true;
    }

    #endregion

    #region Activation

    public void SetActive(bool active)
    {
        if (active == isActive)
            return;

        SetVerticalSpeed(activationSpeed);

        if (active)
        {
            SetTargetY(idleY);
        }
        else
        {
            SetTargetY(inactiveY);
        }

        isActive = active;
    }

    public void Halt()
    {
        if (rb == null)
            return;

        targetY = rb.position.y;
        isMoving = false;

        rb.linearVelocity = Vector2.zero;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    #endregion
}