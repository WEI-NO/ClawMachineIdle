using UnityEngine;


public class GrabButton : InGameButton
{
    protected override void ButtonFunction()
    {
        ClawObject.Instance.StartGrabSequence();
    }
}