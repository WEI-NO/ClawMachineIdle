using UnityEngine;

public class CraneStickController : MonoBehaviour
{
    public Rigidbody2D rb;

    public float moveSpeed;

    [Header("Input Properties")]
    public float xInputSensitivity = 1.0f;
    public float xInputDamping = 1.0f;
    public float xInput;
    public float xVelocity;
   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        XInputUpdate();
    }

    private void FixedUpdate()
    {
        CraneMovementUpdate();
    }

    #region Input

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

        xVelocity = xInput * moveSpeed;
        rb.linearVelocityX = xVelocity;
    }

    #endregion movement

}
