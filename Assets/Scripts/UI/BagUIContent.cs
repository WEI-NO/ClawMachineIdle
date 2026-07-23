using CustomLibrary.References;
using System.Collections.Generic;
using System.Linq;
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
    public static BagUIContent Instance;

    [SerializeField] BagUI bagUI;
    [SerializeField] PlayerInventory inventory;
    SlotItemDisplay slotItemDisplay;

    [SerializeField] BagItemSlot slot;
    [SerializeField] BagItemSlot currentSlot;
    [SerializeField] Transform contentTransform;

    private Dictionary<string, BagItemSlot> addedItems = new Dictionary<string, BagItemSlot>();
    [SerializeField] GameObject bottomPanel;

    private BagItemSlot currentDisplayedItem = null;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        slotItemDisplay = SlotItemDisplay.Instance;
        inventory = PlayerInventory.Instance;

        bagUI.OnUIClose += () => { print("Hello"); bottomPanel.SetActive(false); };
        bagUI.OnUIOpen += () => {
            SelectSlot(currentDisplayedItem);
            if (currentDisplayedItem)
                bottomPanel.SetActive(true); 
        };

        if (inventory)
        {
            inventory.OnBackpackModified += OnNewItemAdded;

            List<InventoryItem> allItems = inventory.Backpack.SelectMany(dict => dict.Values).ToList();

            foreach (var item in allItems)
            {
                OnNewItemAdded(item);
            }
        }
    }

    private void Update()
    {
    }

    private void OnNewItemAdded(InventoryItem newItem)
    {
        string key = newItem.ItemID;
        if (addedItems.ContainsKey(newItem.ItemID))
        {
            // If already added.
            // Update
            if (newItem.quantity <= 0)
            {
                Destroy(addedItems[key].gameObject);
                addedItems.Remove(key);
                
            } else
            {
                addedItems[key].UpdateSlotDisplay();
            }

            if (currentDisplayedItem != null && newItem.ItemID == currentDisplayedItem.GetHeldItem().ItemID)
            {
                SelectSlot(currentDisplayedItem);
            }

        } else
        {
            // If it doesn't exist, Add it
            var slot = Instantiate(this.slot) as BagItemSlot;
            slot.transform.SetParent(contentTransform, false);
            slot.SetItem(newItem);
            addedItems.Add(newItem.ItemID, slot);
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

    public void SelectSlot(BagItemSlot slot)
    {
        if (slot == null)
        {
            currentDisplayedItem = null;
            bottomPanel.SetActive(false);
            return;
        }

        SlotItemDisplay.Instance.DisplayItem(slot.GetHeldItem(), slot);
        currentDisplayedItem = slot;
        bottomPanel.SetActive(true);
    }
}
