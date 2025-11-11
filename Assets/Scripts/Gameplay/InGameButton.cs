using UnityEngine;
using UnityEngine.EventSystems;

public abstract class InGameButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
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
        animator = GetComponent<Animator>();
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

    protected Animator animator;

    [Header("Animator Triggers")]
    public string downTrigger = "Pressed";
    public string startDownTrigger = "Down";

    public void OnPointerDown(PointerEventData eventData)
    {
        animator.SetBool(downTrigger, true);
        animator.SetTrigger(startDownTrigger);

        ButtonFunction();
        //ClawObject.Instance.StartGrabSequence();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        animator.SetBool(downTrigger, false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Optional: handle dragging visuals if needed
    }

    protected virtual void ButtonFunction()
    {

    }
}