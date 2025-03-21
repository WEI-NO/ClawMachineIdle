using UnityEngine;

public class Claw_1 : BaseClaw
{
    float initialY;
    protected override void OnStart()
    {
        initialY = transform.position.y;
    }

    protected override void OnUpdate()
    {
        transform.position = new Vector3(transform.position.x, initialY, transform.position.z);

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

        #endregion temporary controls
    }

    #region Movements



    #endregion movements
}
