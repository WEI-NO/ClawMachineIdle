using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Burst.Intrinsics;
using UnityEditor.Callbacks;
using UnityEngine;

public class ClawArmController : MonoBehaviour
{
    private const float OpenStatePercentage = 1.0f;
    private const float CloseStatePercentage = 0.0f;

    [Header("References")]
    public BaseClaw parentClaw;

    [Header("Arm Properties")]
    public Transform leftArm;
    public Transform rightArm;
    public Transform prizeHolder;
    public float minimumRotation = 0f;
    public float maximumRotation = 80f;

    private float currentStrength;
    private float targetPercentage;

    private ArmState currentState;

    [Header("Prize Properties")]
    public BasePrize grabbedPrize = null;
    public float prizeGrabDelay = 0.5f;

    private void Awake()
    {
        parentClaw = GetComponentInParent<BaseClaw>();
        if (parentClaw)
        {
            //parentClaw.OnClawUp += ClaimPrize;
        }
    }

    private void Start()
    {
        if (leftArm == null || rightArm == null)
        {
            Debug.LogWarning($"{gameObject.name}: Either left or right arm is unassigned/null");
            Destroy(this);
        }

        SetTargetProgress(ArmState.Close, 5.0f, force:true);
    }

    private void FixedUpdate()
    {
        ArmProgressUpdate(0);
        ArmProgressUpdate(1);
    }

    #region Arm Controls

    /// <summary>
    /// 
    ///     Sets the target state for the arms
    /// </summary>
    /// <param name="state"></param>
    /// <param name="strength">How strong it performs the action</param>
    /// <param name="force">Default to false: Set it to true to override if the currentState == state</param>

    public void SetTargetProgress(ArmState state, float strength, bool force = false)
    {
        if (currentState == state && !force) return;

        // Modify strength
        currentStrength = strength;
        targetPercentage = state == ArmState.Open ? OpenStatePercentage : CloseStatePercentage;
        currentState = state;
    }

    /// <summary>
    /// Updates the arm progress, called in FixedUpdate()
    /// </summary>
    /// <param name="armIndex"></param>
    private void ArmProgressUpdate(int armIndex)
    {
        if (armIndex < 0 || armIndex > 1) return;

        Transform armTrans = armIndex == 0 ? leftArm : rightArm;
        Vector3 armEulerAngles = armTrans.localEulerAngles;
        float direction = armIndex == 0 ? -1f : 1f;

        // Normalize zRot to the [-180, 180] range
        float zRot = (armTrans.localEulerAngles.z > 180) ? armTrans.localEulerAngles.z - 360 : armTrans.localEulerAngles.z;

        float targetzRot = (maximumRotation - minimumRotation) * targetPercentage * direction;
        float t = Time.fixedDeltaTime * currentStrength;
        float newzRot = Mathf.Lerp(zRot, targetzRot, t);

        armTrans.GetComponent<Rigidbody2D>().MoveRotation(newzRot);
        //armTrans.localEulerAngles = new Vector3(armEulerAngles.x, armEulerAngles.y, newzRot);
    }

    #endregion arm controls

    #region Prize Control

    private IEnumerator OnPrizeDetected(BasePrize prize)
    {
        if (grabbedPrize != null) yield break;

        //grabbedPrize = prize;
        //prize.ActivateGrabbedState();
        //yield return new WaitForSeconds(prizeGrabDelay);
        //prize.SetTarget(parentClaw.midRaycastPoint);
        //grabbedPrize.transform.SetParent(prizeHolder);
        //targetPercentage = 0.2f;
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (collision.gameObject.layer == LayerMask.NameToLayer("Prizes")) // if it is a prize
        //{
        //    if (grabbedPrize == null && parentClaw.currentPrize == collision.gameObject)
        //    {
        //        BasePrize prize = collision.GetComponent<BasePrize>();
        //        StartCoroutine(OnPrizeDetected(prize));
        //    }
        //}
    }

    private void ClaimPrize()
    {
        if (grabbedPrize)
        {
            Destroy(grabbedPrize.gameObject);
            grabbedPrize = null;
        }
        SetTargetProgress(ArmState.Close, 5.0f);
    }

    #endregion prize control
}
