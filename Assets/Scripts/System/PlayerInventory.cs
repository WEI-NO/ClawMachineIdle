using CustomLibrary.References;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    public BaseItem item;
    public string ItemID;
    public int quantity;

    public InventoryItem(BaseItem item, string id, int quantity)
    {
        this.item = item;
        this.ItemID = id;
        this.quantity = quantity;
    }
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    // Sorted by ItemCategory -> name
    public List<Dictionary<string, InventoryItem>> Backpack = new List<Dictionary<string, InventoryItem>>();
    public Action<InventoryItem> OnBackpackModified;
    private void Awake()
    {
        Initializer.SetInstance(this);

        BackpackInitialize();

        OnBackpackModified += (i) => { PrintBackpack(); };
    }

    #region Backpack

    private void BackpackInitialize()
    {
        for (int i = 0; i < ItemCategory.Count.ToInt(); i++)
        {
            Backpack.Add(new Dictionary<string, InventoryItem>());
        }
    }

    public bool UseItem(BaseItem item, int quantity)
    {
        var bp = GetBackpack(item);

        if (HasItem(item, quantity))
        {
            bp[item.ItemID].quantity -= quantity;
            return true;
        } else
        {
            return false;
        }
    }

    public bool HasItem(BaseItem item, int quantity)
    {
        var bp = GetBackpack(item);

        if (bp == null) return false;

        if (bp.ContainsKey(item.ItemID))
        {
            return quantity <= bp[item.ItemID].quantity;
        } else
        {
            return false;
        }
    }

    public int GetItemCount(string id, ItemCategory category = ItemCategory.None)
    {
        int totalCount = 0;

        if (category != ItemCategory.None)
        {
            // Specific category search
            var bp = Backpack[category.ToInt()];
            if (bp.ContainsKey(id))
            {
                return bp[id].quantity;
            }
        }
        else
        {
            // Search across all categories
            for (int i = 0; i < ItemCategory.Count.ToInt(); i++)
            {
                var bp = Backpack[i];
                if (bp.ContainsKey(id))
                {
                    totalCount += bp[id].quantity;
                }
            }
        }

        return totalCount;

    }

    public void GiveItem(BaseItem item, int quantity)
    {
        var bp = GetBackpack(item);

        if (bp == null) return;

        if (bp.ContainsKey(item.ItemID))
        {
            bp[item.ItemID].quantity += quantity;
        }
        else
        {
            InventoryItem newItem = new InventoryItem(item, item.ItemID, quantity);
            bp.Add(item.ItemID, newItem);
        }

        OnBackpackModified?.Invoke(bp[item.ItemID]);
    }

    private Dictionary<string, InventoryItem> GetBackpack(BaseItem item)
    {
        if (!item) return null;

        return Backpack[item.ItemType.ToInt()];
    }

    private void PrintBackpack()
    {
        string result = "Backpack: \n";
        for (int i = 0; i < ItemCategory.Count.ToInt(); i++)
        {
            foreach (var item in Backpack[i])
            {
                result += $"| {item.Key}, {item.Value.quantity} | ";
            }
        }
        print(result);
    }

    #endregion backpack
}
