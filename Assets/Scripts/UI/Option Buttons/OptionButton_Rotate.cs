using UnityEngine;

public class OptionButton_Rotate : BaseOptionButton
{
    protected override void ActivateFunction()
    {
        if (target is var placeable)
        {
            // Perform function
            placeable.Flip();
        }
    }
}
