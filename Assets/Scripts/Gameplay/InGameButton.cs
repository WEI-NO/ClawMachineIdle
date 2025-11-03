using UnityEngine;
using UnityEngine.EventSystems;

public abstract class InGameButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    protected Animator animator;

    [Header("Animator Triggers")]
    public string downTrigger = "Pressed";
    public string startDownTrigger = "Down";

    protected void Awake()
    {
        animator = GetComponent<Animator>();
    }

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