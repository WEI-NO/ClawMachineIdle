using CustomLibrary.References;
using Mono.Cecil.Cil;
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


    [Header("Stamina Properties")]
    public GameObject repairEffectPrefab;
    public float MaxStaminaPerRefresh = 100;
    public float CurrentStamina = 0;
    public float StaminaPerGrab = 5;
    public Action<float> OnStaminaChange;

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
    }

    private void FixedUpdate()
    {
        HeightUpdate();
    }

    public bool InGrabSequence()
    {
        return currentSequence != null;
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
        CraneStickController.Instance.SetTargetY(CraneStickController.Instance.grabY);
        CraneStickController.Instance.SetVerticalSpeed(downwardSpeed);
        rb.gravityScale = sequenceGravity;
        rb.angularDamping = sequenceAngularDamp;
        heightMoving = true;

        yield return null;
        while (heightMoving)
        {
            yield return null;
        }

        downSequence = false;
        clawC.ChangeState(ClawState.Grab);
        yield return new WaitForSeconds(0.85f);
        OnGrabbed?.Invoke();
        CraneStickController.Instance.SetTargetY(CraneStickController.Instance.idleY);
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
    }


    #region Stamina

    public void RefreshStamina()
    {
        CurrentStamina = MaxStaminaPerRefresh;
        OnStaminaChange?.Invoke(CurrentStamina);
    }
    public bool HasStamina()
    {
        return CurrentStamina >= StaminaPerGrab;
    }

    public bool UseStamina()
    {
        if (!HasStamina())
        {
            return false;
        }

        CurrentStamina -= StaminaPerGrab;
        OnStaminaChange?.Invoke(CurrentStamina);
        return true;
    }

    #endregion stamina
}
