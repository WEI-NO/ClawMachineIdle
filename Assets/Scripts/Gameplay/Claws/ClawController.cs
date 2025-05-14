using System;
using UnityEngine;

public enum ClawState
{
    None = -1,
    Relax,
    Expand,
    Grab
}

public class ClawController : MonoBehaviour
{
    [Header("Components")]
    public HingeJoint2D l_prong;
    public HingeJoint2D r_prong;

    [Header("Claw Properties")]
    public ClawSettings relaxSetting;
    public ClawSettings expandSetting;
    public ClawSettings grabSetting;

    public Action OnGrabComplete;
    public Action OnExpandComplete;

    [Header("Completion Detection")]
    public float angleThreshold = 1f; // degrees

    [Header("State Properties")]
    public ClawState state = ClawState.None;


    void Start()
    {
        ChangeState(ClawState.Relax);
        OnGrabComplete += () => { print("grab complete!"); };
        OnExpandComplete += () => { print("expand complete!"); };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeState(ClawState.Relax);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeState(ClawState.Expand);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeState(ClawState.Grab);
        }

        CheckIfClawCompleted();
    }

    private void CheckIfClawCompleted()
    {
        if (state != ClawState.Grab && state != ClawState.Expand)
            return;

        float leftAngle = l_prong.jointAngle;
        float rightAngle = r_prong.jointAngle;

        float leftTarget = state == ClawState.Grab ? grabSetting.abs_angleRange.y : expandSetting.abs_angleRange.y;
        float rightTarget = state == ClawState.Grab ? -grabSetting.abs_angleRange.y : -expandSetting.abs_angleRange.y;

        bool leftClose = Mathf.Abs(leftAngle - leftTarget) <= angleThreshold;
        bool rightClose = Mathf.Abs(rightAngle - rightTarget) <= angleThreshold;

        if (leftClose && rightClose)
        {
            if (state == ClawState.Grab && OnGrabComplete != null)
            {
                OnGrabComplete.Invoke();
                state = ClawState.None; // prevent repeat calls
            }
            else if (state == ClawState.Expand && OnExpandComplete != null)
            {
                OnExpandComplete.Invoke();
                state = ClawState.None;
            }
        }
    }

    public void SetClawState(bool isTrigger)
    {
        l_prong.GetComponent<Collider2D>().isTrigger = isTrigger;
        r_prong.GetComponent<Collider2D>().isTrigger = isTrigger;
    }

    #region Settings

    public void UpdateClawSetting(ClawSettings setting)
    {
        // Motor
        // Left
        var m = l_prong.motor;
        m.motorSpeed = setting.motorStrength * (setting.motorDir == 0 ? 1.0f : setting.motorDir);
        l_prong.motor = m;
        l_prong.useMotor = setting.motorStrength == 0 ? false : setting.useMotor;
        // Right
        m = r_prong.motor;
        m.motorSpeed = setting.motorStrength * (setting.motorDir == 0 ? 1.0f : setting.motorDir) * -1.0f;
        r_prong.motor = m; 
        r_prong.useMotor = setting.motorStrength == 0 ? false : setting.useMotor;

        // Angle Limits
        // Left
        var al = l_prong.limits;
        float x = -setting.abs_angleRange.x, y = setting.abs_angleRange.y;
        al.min = x; al.max = y;
        l_prong.limits = al;
        // Right
        al = r_prong.limits;
        x = -setting.abs_angleRange.y; y = setting.abs_angleRange.x;
        al.min = x; al.max = y;
        r_prong.limits = al;
    }

    #endregion settings

    #region State

    public void ChangeState(ClawState state)
    {
        if (this.state == state)
        {
            return;
        }

        this.state = state;
        switch (state)
        {
            case ClawState.Relax:
                UpdateClawSetting(relaxSetting);
                break;
            case ClawState.Expand:
                UpdateClawSetting(expandSetting);
                break;
            case ClawState.Grab:
                UpdateClawSetting(grabSetting);
                break;
        }
    }

    #endregion state
}

[System.Serializable]
public class ClawSettings
{
    [Header("Claw Properties")]
    public Vector2 abs_angleRange;
    public bool useMotor;
    public float motorStrength;
    public int motorDir;
}
