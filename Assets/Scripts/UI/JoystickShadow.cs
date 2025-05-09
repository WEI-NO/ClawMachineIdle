using UnityEngine;

public class JoystickShadow : MonoBehaviour
{
    [Header("Shadow Properties")]
    [SerializeField] private RectTransform joystickMain;

    public void Update()
    {
        // Reverse the rotation on z from main joystick
        if (!joystickMain) return;

        transform.localRotation = Quaternion.Euler(0, 0, -joystickMain.localEulerAngles.z);
    }
}
