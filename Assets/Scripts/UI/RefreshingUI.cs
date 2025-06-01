using UnityEngine;

public class RefreshingUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Activate()
    {
        if (!anim) return;
        anim.SetTrigger("Activate");
    }

    public void Deactivate()
    {
        if (!anim) return;

        anim.SetTrigger("Deactivate");
    }
    
}