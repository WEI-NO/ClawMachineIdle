using System.Collections.Generic;
using UnityEngine;

public enum BagCategory
{
    All,
    Plushie,
    Furnitures,
    Items,
    Count
}

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

    public void ShowCategory(BagCategory category)
    {
        foreach (var slot in addedItems.Values)
        {
            switch (category)
            {
                case BagCategory.All:
                    slot.gameObject.SetActive(true);
                    break;
                case BagCategory.Plushie:
                    if (slot.GetItem() is var i)
                    {
                        if (i is PlaceableItem placeable)
                        {
                            if (placeable.isPlushie)
                            {
                                slot.gameObject.SetActive(true);
                                break;
                            }
                        }
                    }
                    slot.gameObject.SetActive(false);
                    break;
                case BagCategory.Furnitures:
                    if (slot.GetItem() is var j)
                    {
                        if (j is PlaceableItem placeable)
                        {
                            if (!placeable.isPlushie)
                            {
                                slot.gameObject.SetActive(true);
                                break;
                            }
                        }
                    }
                    slot.gameObject.SetActive(false);
                    break;
                case BagCategory.Items:
                    if (slot.GetItem() is var k)
                    {
                        if (k.ItemType != ItemCategory.Building)
                        {
                            slot.gameObject.SetActive(true);
                            break;
                        }
                    }
                    slot.gameObject.SetActive(false);
                    break;
            }
        }

    }

}
