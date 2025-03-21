using UnityEngine;

public class Claw_1 : BaseClaw
{
    protected override void OnStart()
    {

    }

    protected override void OnUpdate()
    {
        #region Temporary Controls (PC)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            armController.SetTargetProgress(ArmState.Open, armOpenStrength);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            armController.SetTargetProgress(ArmState.Close, armCloseStrength);
        }

        if (Input.GetKey(KeyCode.A))
        {
            XInput(false);
        }
        if (Input.GetKey(KeyCode.D))
        { 
            XInput(true); 
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartGrabSequence();
        }

        #endregion temporary controls
    }

    #region Movements



    #endregion movements
}
