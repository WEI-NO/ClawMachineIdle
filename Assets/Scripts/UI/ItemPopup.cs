using UnityEngine;
using CustomLibrary.References;

public class ItemPopup : Base_ItemPopup<BaseItem>
{
    public static ItemPopup Instance;

    protected override void AddItem_Internal<T>(T item)
    {
        if (item is BaseItem i)
        {
            incomingItemList.Add(i);
            incomingQuantityList.Add(1);

            totalIncomingItems++;
            itemCounter.text = $"{currentIndex}/{totalIncomingItems}";
            if (!viewIsEnabled)
            {
                StartView();
            }
        }
    }

    protected override void OnAwake()
    {
        Initializer.SetInstance(this);
    }

    public override void Function_One()
    {
        // Animation
        if (anim)
        {
            anim.SetTrigger("End");
        }

        if (incomingItemList == null)
        {
            return;
        }

        PlayerInventory.Instance.GiveItem(incomingItemList[incomingItemIndex], incomingQuantityList[incomingItemIndex]);
        incomingItemList.RemoveAt(incomingItemIndex);
        incomingQuantityList.RemoveAt(incomingItemIndex);

        StartView();
    }

    public override void Function_Two()
    {
        base.Function_Two();
    }

    public override void Function_Three()
    {
        base.Function_Three();
    }
}
