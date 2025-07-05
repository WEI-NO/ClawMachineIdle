using UnityEngine;

public abstract class BaseOptionButton : MonoBehaviour
{
    public IsometricBuilding target;
    public int priority; // Right to left (Lower means right)
    public void Initialize(IsometricBuilding target)
    {
        this.target = target;
        Init(target);
    }

    protected virtual void Init(IsometricBuilding target) { }

    public void ActivateFunc()
    {
        ActivateFunction();
    }

    protected abstract void ActivateFunction();
}
