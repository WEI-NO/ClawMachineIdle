using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class GameModule : MonoBehaviour
{
    #region Base Class
    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnDestroyed() { }

    private void Awake()
    {
        OnAwake();
    }
    private void Start()
    {
        OnStart();
    }
    private void Update()
    {
        OnUpdate();
    }
    private void FixedUpdate()
    {
        OnFixedUpdate();
    }
    #endregion base class

    public void SetEnabled(bool state)
    {
        if (this.enabled == state) return;

        this.enabled = state;
        if (state) OnEnabled();
        else OnDisabled();
    }

    protected virtual void OnEnabled() 
    {
        
    }

    protected virtual void OnDisabled() 
    {

    }


}
