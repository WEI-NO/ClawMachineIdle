using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class QueuePanelDisplay : OpenableUI
{
    [Header("References")]
    public QueueSlot slotPrefab;
    public Transform content;

    [Header("Selection")]
    private List<QueueSlot> selections = new();

    protected override void ToggledOn()
    {
        RefreshContent();
    }

    private void RefreshContent()
    {
        ClearContent();

        var prizes = PlayerInventory.Instance.GetBackpack(ItemCategory.Prize);
        foreach  (var p in prizes)
        {
            if (p.Value.item is EggItem)
            {
                for (int i = 0; i < p.Value.quantity; i++)
                {
                    var newSlot = Instantiate(slotPrefab, content);
                    newSlot.Initialize(p.Value);
                    newSlot.OnSelect += OnSelect;
                }
            }
        }

    }

    private void OnSelect(QueueSlot slot, bool add)
    {
        var controller = IncubationController.Instance;
        int available = controller.GetAvailableSlots();
        if (add)
        {
            if (available > selections.Count)
            {
                selections.Add(slot);
                slot.UpdateVisual(true);
            }
            else
            {
                ErrorDisplayController.AddMessage("Can not select more to incubate!");
                slot.UpdateVisual(false);
            }
        } else
        {
            var index = selections.FindIndex(i => i == slot);
            slot.UpdateVisual(false);
            if (index >= 0 && index < selections.Count) 
                selections.RemoveAt(index);
        }
    }

    private void ClearContent()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        selections = new();
    }

    public void ConfirmAddToQueue()
    {
        var controller = IncubationController.Instance;
        var inventory = PlayerInventory.Instance;

        foreach (var egg in selections)
        {
            var item = egg.assignedItem.item;
            controller.AddToQueue(item);
            inventory.UseItem(item, 1);
        }
        Toggle_Off();
    }
}
