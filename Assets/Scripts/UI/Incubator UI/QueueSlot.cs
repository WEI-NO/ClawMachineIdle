using System;
using UnityEngine;
using UnityEngine.UI;

public class QueueSlot : MonoBehaviour
{
    [Header("Components")]
    public Image icon;
    public GameObject selectionIndicator;

    [Header("References")]
    public InventoryItem assignedItem;
    [SerializeField] private bool selected = false;
    public Action<QueueSlot, bool> OnSelect;

    public void Initialize(InventoryItem item)
    {
        if (item.item is not EggItem)
        {
            Destroy(gameObject);
            return;
        }

        assignedItem = item;
        icon.sprite = assignedItem.item.ItemIcon;
        UpdateVisual(false);
    }

    public void UpdateVisual(bool selected)
    {
        if (selected)
        {
            selectionIndicator.SetActive(true);
            this.selected = true;
        }
        else
        {
            selectionIndicator.SetActive(false);
            this.selected = false;
        }
    }

    public void Select()
    {
        selected = !selected;
        OnSelect?.Invoke(this, selected);
    }
}
