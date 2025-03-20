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

        if (Input.GetKeyDown(KeyCode.Q))
        {
            armController.SetTargetProgress(1.0f, 5.0f);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            armController.SetTargetProgress(0, 10.0f);
        }


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
