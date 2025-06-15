using UnityEngine;

public class SI_PixelOutline : ShaderInstancer
{
    protected override void ChangeFloat_1(float f)
    {
        sr.material.SetFloat("_Radius", f);
    }

    protected override float GetFloat_1()
    {
        return sr.material.GetFloat("_Radius");
    }
}
