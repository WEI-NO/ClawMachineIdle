using CustomLibrary.References;
using NUnit.Framework.Internal.Commands;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ClawObject : MonoBehaviour
{
    public static ClawObject Instance;

    [Header("References")]
    public Rigidbody2D rb;
    public ClawController clawC;
    public PrizeDetector detector;

    [Header("Vertical Properties")]
    //public float defaultY = -1.0f;
    //public float targetY = -1.0f;
    //public float verticalStrength = 1.0f;
    //public Vector2 yLimits = new Vector2(-5.0f, -1.0f);
    public bool heightMoving = false;
    public float downwardSpeed;
    public Vector2 upwardSpeedRange = new Vector2(1.0f, 2.0f);
    public float upwardSpeed;
    private HingeJoint2D selfHinge;


    [Header("Swing Prevention")]
    public float defaultGravity = 5;
    public float defaultAngularDamp = 3;
    public float sequenceGravity = 100, sequenceAngularDamp = 100;

    [Header("Grab Sequence Properties")]
    public bool inSequence = false;
    private Coroutine currentSequence = null;
    public Action OnGrabSequenceStart;
    public Action OnGrabbed;
    public Action OnClaimPrize;
    public bool downSequence = false;


    

    private void Awake()
    {
        Initializer.SetInstance(this);

        selfHinge = GetComponent<HingeJoint2D>();
        rb = GetComponent<Rigidbody2D>();
        detector = GetComponentInChildren<PrizeDetector>();
    }

    private void Start()
    {
        //targetY = defaultY;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartGrabSequence();
        }

        float rarityMultiplier = 0.25f;
        float totalMultiplier = 0.0f;
        foreach (var i in detector.grabbedObj)
        {
            if (i.RewardItem == null) continue;
            totalMultiplier += (i.RewardItem.itemRarity.ToInt()+1) * rarityMultiplier;
        }

        upwardSpeed = Mathf.Clamp(upwardSpeedRange.y - totalMultiplier, upwardSpeedRange.x, upwardSpeedRange.y);
        //if (Input.GetKey(KeyCode.UpArrow))
        //{
        //    targetY += Time.deltaTime * verticalStrength;
        //}

        //if (Input.GetKey(KeyCode.DownArrow))
        //{
        //    targetY -= Time.deltaTime * verticalStrength;
        //}

        //targetY = Mathf.Clamp(targetY, yLimits.x, yLimits.y);
    }

    private void FixedUpdate()
    {
        HeightUpdate();
    }

    public void StartGrabSequence()
    {
        if (currentSequence == null)
        {
            currentSequence = StartCoroutine(GrabSequence());
            OnGrabSequenceStart?.Invoke();
        }
    }

    public void ExpediteGrab()
    {
        if (!inSequence || !downSequence) return;
        //targetY = selfHinge.connectedAnchor.y;
        CraneStickController.Instance.Halt();
    }

    private IEnumerator GrabSequence()
    {
        downSequence = true;
        inSequence = true;
        clawC.ChangeState(ClawState.Expand);
        //targetY = yLimits.x;
        CraneStickController.Instance.SetTargetY(CraneStickController.Instance.yLimits.x);
        CraneStickController.Instance.SetVerticalSpeed(downwardSpeed);
        heightMoving = true;
        rb.angularDamping = sequenceAngularDamp;
        rb.gravityScale = sequenceGravity;

        yield return null;
        while (heightMoving)
        {
            yield return null;
        }

        downSequence = false;
        clawC.ChangeState(ClawState.Grab);
        yield return new WaitForSeconds(0.85f);
        OnGrabbed?.Invoke();
        CraneStickController.Instance.SetTargetY(CraneStickController.Instance.yLimits.y);
        CraneStickController.Instance.SetVerticalSpeed(upwardSpeed);
        //targetY = -1.0f;
        heightMoving = true;
        yield return null;
        while (heightMoving)
        {
            yield return null;
        }

        OnClaimPrize?.Invoke();

        rb.angularDamping = defaultAngularDamp;
        rb.gravityScale = defaultGravity;
        //clawC.ChangeState(ClawState.Relax);
        currentSequence = null;
        inSequence = false;
        clawC.SetClawState(false);
    }

    /// <summary>
    /// Updates the height of the crane. Should be called in FixedUpdate();
    /// </summary>
    private float heightVelocity; // Needs to be a field (not local)

    private void HeightUpdate()
    {
        heightMoving = CraneStickController.Instance.isMoving;
        //if (!selfHinge) return;

        //var c_anchor = selfHinge.connectedAnchor;
        //float currentY = c_anchor.y;

        //if (Mathf.Abs(targetY - currentY) <= 0.01f)
        //{
        //    c_anchor.y = targetY;
        //    heightVelocity = 0f;
        //    heightMoving = false;
        //}
        //else
        //{
        //    c_anchor.y = Mathf.SmoothDamp(currentY, targetY, ref heightVelocity, 0.35f, verticalStrength);
        //    heightMoving = true;
        //}

        //c_anchor.y = Mathf.Clamp(c_anchor.y, yLimits.x, yLimits.y);
        //print(c_anchor.y);
        //selfHinge.connectedAnchor = c_anchor;
    }

}
