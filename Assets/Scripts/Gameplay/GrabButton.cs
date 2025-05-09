using UnityEngine;
using UnityEngine.EventSystems;

public class GrabButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Animator animator;

    [Header("Animator Triggers")]
    public string downTrigger = "Pressed";
    public string startDownTrigger = "Down";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        animator.SetBool(downTrigger, true);
        animator.SetTrigger(startDownTrigger);

        ClawObject.Instance.StartGrabSequence();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        animator.SetBool(downTrigger, false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Optional: handle dragging visuals if needed
    }
}
