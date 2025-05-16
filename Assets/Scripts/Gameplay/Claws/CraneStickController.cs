using CustomLibrary.References;
using JetBrains.Annotations;
using UnityEngine;

public class CraneStickController : MonoBehaviour
{
    public static CraneStickController Instance;
    public Rigidbody2D rb;

    public float moveSpeed;
    public ClawObject claw;

    [Header("Input Properties")]
    public float xInputSensitivity = 1.0f;
    public float xInputDamping = 1.0f;
    public float xInput;
    public float xVelocity;
    public Vector2 xLimits = new Vector2();

    [Header("Target Height")]
    public float targetY;
    public Vector2 yLimits = new Vector2(-5f, 5f);

    [Header("Movement Settings")]
    public float verticalSpeed = 5f;
    public float arriveThreshold = 0.05f;

    public bool isMoving = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Initializer.SetInstance(this);
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        SetTargetY(yLimits.y);
        CalculateHorizontalLimits();
    }

    // Update is called once per frame
    void Update()
    {
        //XInputUpdate();
        if (Input.GetKey(KeyCode.DownArrow))
        {
            SetTargetY(targetY - 10.0f * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            SetTargetY(targetY + 10.0f * Time.deltaTime);
        }

        if (rb)
        {
            if (isMoving)
            {
                // Ensure FreezePositionY is OFF (allow vertical movement)
                rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
            }
            else
            {
                // Ensure FreezePositionY is ON (lock vertical position)
                rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
            }
        }
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
        xInput = input;
    }

    /// <summary>
    /// Updates the X Input
    /// </summary>
    private void XInputUpdate()
    {
        xInput = 0;
        bool hasInput = false;
        if (Input.GetKey(KeyCode.A))
        {
            xInput += -1.0f * xInputSensitivity;
            hasInput = true;
        }

        if (Input.GetKey(KeyCode.D))
        {
            xInput += 1.0f * xInputSensitivity;
            hasInput = true;
        }

        xInput = Mathf.Clamp(xInput, -1.0f, 1.0f);
        if (!hasInput)
        {
            xInput = Mathf.Lerp(xInput, 0.0f, Time.deltaTime * xInputDamping);
            if (Mathf.Abs(xInput) <= 0.001f)
            {
                xInput = 0.0f;
            }
        }
    }

    #endregion input

    #region Movement

    /// <summary>
    /// Called in FixedUpdate() Moves x velocity based on the x input.
    /// </summary>
    public void CraneMovementUpdate()
    {
        if (!rb) return;

        if (claw && claw.inSequence)
        {
            xVelocity = 0.0f;
        }
        else
        {
            float proposedX = rb.position.x + xInput * moveSpeed * Time.fixedDeltaTime;

            // If input would move outside limits, ignore
            if ((xInput < 0f && rb.position.x <= xLimits.x) ||
                (xInput > 0f && rb.position.x >= xLimits.y))
            {
                xVelocity = 0.0f;
            }
            else
            {
                xVelocity = xInput * moveSpeed;
            }
        }

        rb.linearVelocityX = xVelocity;
    }

    public void VerticalMovementUpdate()
    {
        if (!isMoving) return;

        Vector2 position = rb.position;
        float distance = targetY - position.y;

        if (Mathf.Abs(distance) <= arriveThreshold)
        {
            rb.linearVelocity = Vector2.zero;
            rb.MovePosition(new Vector2(position.x, targetY)); // Snap to final position
            isMoving = false;
            return;
        }

        float direction = Mathf.Sign(distance);
        float newY = position.y + direction * verticalSpeed * Time.fixedDeltaTime;
        newY = Mathf.Clamp(newY, yLimits.x, yLimits.y);

        rb.MovePosition(new Vector2(position.x, newY));
    }

    public void SetVerticalSpeed(float speed)
    {
        verticalSpeed = speed;
    }

    public void SetTargetY(float newY)
    {
        targetY = Mathf.Clamp(newY, yLimits.x, yLimits.y);
        isMoving = true;
    }

    public void Halt()
    {
        targetY = rb.position.y;
        isMoving = false;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    private void ClampXPosition()
    {
        Vector2 pos = rb.position;
        if (pos.x < xLimits.x || pos.x > xLimits.y)
        {
            pos.x = Mathf.Clamp(pos.x, xLimits.x, xLimits.y);
            rb.position = new Vector2(pos.x, rb.position.y);
            rb.linearVelocityX = 0f; // prevent sliding after clamp
        }
    }

    private void CalculateHorizontalLimits()
    {
        Camera cam = Camera.main;
        float screenHalfWidth = cam.orthographicSize * cam.aspect;

        float halfCraneWidth = 0.5f;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            halfCraneWidth = sr.bounds.extents.x;
        }

        xLimits = new Vector2(-screenHalfWidth + halfCraneWidth, screenHalfWidth - halfCraneWidth);
    }

    #endregion movement

}
