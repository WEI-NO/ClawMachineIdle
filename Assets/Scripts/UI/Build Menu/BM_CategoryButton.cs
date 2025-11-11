using CustomLibrary.References;
using System;
using UnityEngine;
using UnityEngine.UI;

public class BM_CategoryButton : MonoBehaviour
{
    [Header("Components")]
    private Animator anim;
    private Button button;
    public Action<int> OnPressed;

    public BM_Category category;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(() => { OnPressed?.Invoke(category.ToInt()); });
    }

    public void Initialize(BM_Category category)
    {
        this.category = category;
    }

    public void SetDisable(bool state)
    {
        if (state)
        {
            button.interactable = true;
        } else
        {
            button.interactable = false;
        }
    }
}
