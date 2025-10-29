using CustomLibrary.References;
using TMPro;
using UnityEngine;

public class EditModeUI : MonoBehaviour
{
    public static EditModeUI Instance;

    private Animator anim;
    

    private void Awake()
    {
        Initializer.SetInstance(this);
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        if (TouchController.Instance)
        {
            //TouchController.Instance.OnEditModeEnter += () => { Toggle(true); };
            //TouchController.Instance.OnEditModeExit += () => { Toggle(false); };
        }
    }

    public void Toggle(bool state)
    {
        if (anim)
        {
            anim.SetTrigger(state ? "Show" : "Hide");
        }

        if (MainHUD_UI.Instance)
        {
            MainHUD_UI.Instance.ToggleState(!state);
        }
    }
}
