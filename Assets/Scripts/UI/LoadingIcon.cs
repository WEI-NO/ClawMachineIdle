using UnityEngine;

public class LoadingIcon : MonoBehaviour
{
    [Header("Components")]
    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }
    
    public void TriggerEnd()
    {
        _anim.SetTrigger("End");
    }

}
