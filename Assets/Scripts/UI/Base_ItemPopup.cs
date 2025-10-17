using CustomLibrary.References;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public abstract class Base_ItemPopup<T> : MonoBehaviour
{

    [Header("References")]
    protected Animator anim;
    [SerializeField] protected Image prizeIcon;
    [SerializeField] protected TextMeshProUGUI itemCounter;
    protected List<BaseItem> incomingItemList = new List<BaseItem>();
    protected List<int> incomingQuantityList = new List<int>();
    protected bool viewIsEnabled = false;
    protected int incomingItemIndex = 0;
    protected int totalIncomingItems = 0;
    protected int currentIndex = 0;
    [SerializeField] private float iconSizeMultiplier = 1.0f;

    protected virtual void OnAwake() { }
    protected void Awake()
    {
        OnAwake();

        anim = GetComponent<Animator>();
    }

    public void AddItem<T>(T item)
    {
        if (item == null) return;

        AddItem_Internal(item);
    }

    protected abstract void AddItem_Internal<T>(T item);


    // Should normally pass in an Egg
    public void StartView()
    {
        if (incomingItemList.Count <= 0)
        {
            totalIncomingItems = 0;
            currentIndex = 0;
            incomingItemIndex = 0;
            viewIsEnabled = false;
            return;
        }


        viewIsEnabled = true;
        currentIndex++;

        itemCounter.text = $"{currentIndex}/{totalIncomingItems}";

        Sprite prizeSprite = incomingItemList[incomingItemIndex].ItemIcon;
        prizeIcon.sprite = prizeSprite;
        // Get the RectTransform component of the prizeIcon
        RectTransform iconRect = prizeIcon.GetComponent<RectTransform>();
        // Get the sprite's original pixel size
        Vector2 spriteSize = prizeSprite.rect.size;
        // Calculate the new size by applying the multiplier
        Vector2 newSize = spriteSize * iconSizeMultiplier;
        // Set the RectTransform's sizeDelta to match the new size
        iconRect.sizeDelta = newSize;

        // Animation
        if (anim)
        {
            anim.ResetTrigger("End");
            viewIsEnabled = true;
            anim.SetTrigger("Start");
        }
    }

    public virtual void Function_One()
    {

    }

    public virtual void Function_Two()
    {

    }

    public virtual void Function_Three()
    {

    }



    private void EndSequence()
    {
        if (incomingItemList.Count >= 1 && incomingItemList[incomingItemIndex] != null)
        {
            StartView();
        }
    }

}
