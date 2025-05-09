using UnityEngine;

public class MenuContainer : MonoBehaviour
{
    [Header("Components")]
    private Animator anim;

    [Header("Menu Properties")]
    private bool _isOpened = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        ForceSetMenu(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Button Functions

    public void ToggleMenu()
    {
        ForceSetMenu(!_isOpened);
    }

    public void ForceSetMenu(bool state)
    {
        anim.SetTrigger(state ? "Open" : "Close");
        _isOpened = state;
    }

    #endregion button functions
}
