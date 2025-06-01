using System.Collections.Generic;
using UnityEngine;

public class BagUIContent : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;

    [SerializeField] private BagItemSlot slot;
    [SerializeField] private Transform contentTransform;

    private Dictionary<string, BagItemSlot> addedItems = new Dictionary<string, BagItemSlot>();

    private void Start()
    {
        inventory = PlayerInventory.Instance;
        if (inventory)
        {
            inventory.OnBackpackModified += OnNewItemAdded;
        }
    }

    private void OnNewItemAdded(InventoryItem newItem)
    {
        string key = newItem.itemName;
        if (addedItems.ContainsKey(newItem.itemName))
        {
            // If already added.
            // Update
            addedItems[key].UpdateSlotDisplay();
        } else
        {
            // If it doesn't exist, Add it
            var slot = Instantiate(this.slot) as BagItemSlot;
            slot.transform.SetParent(contentTransform, false);
            slot.SetItem(newItem);
            addedItems.Add(newItem.itemName, slot);
        }

    }

}
