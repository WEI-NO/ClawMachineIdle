using CustomLibrary.References;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EggPopup : Base_ItemPopup<BasePrize>
{
    public static EggPopup Instance;

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
        if (IncubationController.Instance.MaxQueueSpace())
        {
            ErrorDisplayController.AddMessage("Queue is full!");
            return;
        }

        // Animation
        if (anim)
        {
            anim.SetTrigger("End");
        }

        if (incomingItemList == null)
        {
            return;
        }

        IncubationController.Instance.AddToQueue(incomingItemList[incomingItemIndex]);
        incomingItemList.RemoveAt(incomingItemIndex);
        incomingQuantityList.RemoveAt(incomingItemIndex);

        StartView();
    }

    protected override void AddItem_Internal<T>(T item)
    {
        if (item is BasePrize prize)
        {
            incomingItemList.Add(prize.RewardItem);
            incomingQuantityList.Add(prize.Quantity);

            totalIncomingItems++;
            itemCounter.text = $"{currentIndex}/{totalIncomingItems}";
            if (!viewIsEnabled)
            {
                StartView();
            }
        }
    }
}
