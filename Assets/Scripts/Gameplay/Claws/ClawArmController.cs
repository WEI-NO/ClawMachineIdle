using UnityEditor.Callbacks;
using UnityEngine;

public class ClawArmController : MonoBehaviour
{
    private const float OpenStatePercentage = 1.0f;
    private const float CloseStatePercentage = 0.0f;

    [Header("Arm Properties")]
    public Transform leftArm;
    public Transform rightArm;
    public float minimumRotation = 0f;
    public float maximumRotation = 80f;

    private float currentStrength;
    private float targetPercentage;

    private void Start()
    {
        if (leftArm == null || rightArm == null)
        {
            Debug.LogWarning($"{gameObject.name}: Either left or right arm is unassigned/null");
            Destroy(this);
        }
    }

    private void FixedUpdate()
    {
        ArmProgressUpdate(0);
        ArmProgressUpdate(1);
    }

    #region Arm Controls

    // == Set Target Progress ==
    // Desc:
    //          Sets the target progress of the arms,
    //          0.0f to 1.0f : 0.0 is closed, 1.0f is fully opened
    public void SetTargetProgress(ArmState state, float strength)
    {
        // Modify strength
        currentStrength = strength;
        targetPercentage = state == ArmState.Open ? OpenStatePercentage : CloseStatePercentage;
    }

    // == Arm Progress Update ==
    // Desc:
    //          Updates the arms rotation to the target percentage.
    //          Arm Index: 0 = left, 1 = right
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

        armTrans.localEulerAngles = new Vector3(armEulerAngles.x, armEulerAngles.y, newzRot);
    }

    #endregion arm controls
}
