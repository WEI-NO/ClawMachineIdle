using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class OpenableUI : MonoBehaviour
{
    #region Base Class
    protected virtual void OnAwake() { }
    protected virtual void OnEnabled() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnDisabled() { }
    protected virtual void OnDestroyed() { }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        
        active = !startActive;
        ManualToggle();

        OnAwake();
    }
    private void OnEnable()
    {
        OnEnabled();
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
    private void OnDisable()
    {
        OnDisabled();
    }
    private void OnDestroy()
    {
        OnDestroyed();
    }
    #endregion base class

    private Animator anim;
    public bool active = false;
    public List<GameObject> manualToggleObjects;
    public bool startActive = false;
    [Header("Animation Controls")]
    public bool useAnimation = true;
    [SerializeField] private string enableTriggerName = "Open";
    [SerializeField] private string disableTriggerName = "Close";

    public virtual void Toggle()
    {
        if (!useAnimation)
        {
            ManualToggle();
            return;
        }
        
        active = !active;
        if (active)
        {
            anim.SetTrigger(enableTriggerName);
        }
        else
        {
            anim.SetTrigger(disableTriggerName);
        }
    }

    public void Toggle_On()
    {
        if (!active)
        {
            ManualToggle();
        }
    }

    public void Toggle_Off()
    {
        if (!active)
        {
            ManualToggle();
        }
    }

    private void ManualToggle()
    {
        active = !active;
        if (active)
        {
            foreach (GameObject g in  manualToggleObjects)
            {
                g.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject g in manualToggleObjects)
            {
                g.SetActive(false);
            }
        }
    }
}
