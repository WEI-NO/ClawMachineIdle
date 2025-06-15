using NUnit.Framework;
using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public abstract class ShaderInstancer : MonoBehaviour
{
    const int FloatFieldSupportCount = 3;
    [SerializeField] protected Material material;
    protected SpriteRenderer sr;

    [SerializeField] private bool smoothMode = false;
    private float[] targetFloatValues = new float[FloatFieldSupportCount];
    [SerializeField] private float smoothSpeed;
    private Action<float>[] floatChangeCalls = new Action<float>[FloatFieldSupportCount];

    protected void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr && material)
        {
            sr.material = Instantiate(material);
        }
        floatChangeCalls[0] = ChangeFloat_1;
        floatChangeCalls[1] = ChangeFloat_2;
        floatChangeCalls[2] = ChangeFloat_3;
    }

    protected void Update()
    {
        if (smoothMode)
        {
            // Smooth Float
            for (int i = 1; i <= 3; i++)
            {
                float lerpedValue = Mathf.Lerp(GetFloat(i), targetFloatValues[i-1], Time.deltaTime * smoothSpeed);
                floatChangeCalls[i-1]?.Invoke(lerpedValue);
            }
        }
    }

    public void SetFloat(int index, float value)
    {
        if (smoothMode)
        {
            targetFloatValues[index - 1] = value;
        } else
        {
            switch (index)
            {
                case 1:
                    ChangeFloat_1(value);
                    break;
                case 2:
                    ChangeFloat_2(value);
                    break;
                case 3:
                    ChangeFloat_3(value);
                    break;
                default:
                    ChangeFloat_1(value);
                    break;
            }
        }

    }

    public float GetFloat(int index)
    {
        switch (index)
        {
            case 1:
                return GetFloat_1();
            case 2:
                return GetFloat_2();
            case 3:
                return GetFloat_3();
            default:
                return GetFloat_1();
        }
    }

    protected virtual void ChangeFloat_1(float f) { }
    protected virtual void ChangeFloat_2(float f) { }
    protected virtual void ChangeFloat_3(float f) { }

    protected virtual float GetFloat_1() { return 0; }
    protected virtual float GetFloat_2() { return 0; }
    protected virtual float GetFloat_3() { return 0; }
}
