using UnityEngine;

public class SideButtons_UI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator panelAnimator;

    [Header("State")]
    [SerializeField] bool active = false;

    private void Start()
    {
        SetActive(active, true);
    }

    public void SetActive(bool newState, bool force = false)
    {
        if (active == newState && force) return;

        active = newState;
        panelAnimator.SetBool("Open", active);
    }

    public void TogglePanel()
    {
        SetActive(!active);
    }
}
