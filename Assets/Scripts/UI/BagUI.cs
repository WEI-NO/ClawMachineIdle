using UnityEngine;

public class BagUI : MonoBehaviour
{
    private Animator anim;
    public bool active = false;


    void Awake()
    {
        anim = GetComponent<Animator>();        
    }

    public void Toggle()
    {
        active = !active;
        if (active)
        {
            anim.SetTrigger("Open");
        } else
        {
            anim.SetTrigger("Close");
        }
    }
}
