using UnityEngine;

public class Claw_1 : BaseClaw
{


    protected override void OnUpdate()
    {
        if (Input.GetKey(KeyCode.A))
        {
            XInput(false);
        }
        if (Input.GetKey(KeyCode.D))
        { 
            XInput(true); 
        }
    }

    #region Movements



    #endregion movements
}
