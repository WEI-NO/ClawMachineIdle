using UnityEngine;

public class MovementStick : MonoBehaviour
{
    [Header("Function Properties")]
    public BaseClaw currClaw;

    [Header("Joystick")]
    public JoystickTouchController joystick;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (joystick)
        {
            joystick.OnJoystickProgressUpdate += UpdateClawInput;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateClawInput(float progress)
    {
        if (!currClaw) return;
        currClaw.XInput(progress < 0, Mathf.Abs(progress), true);
    }
}
