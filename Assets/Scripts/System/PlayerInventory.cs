using CustomLibrary.References;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class InventoryItem
{
    public BaseItem item;
    public string ItemID;
    public int quantity;

    public string InstanceID { get; private set; }

    public InventoryItem(BaseItem item, string id, int quantity)
    {
        this.item = item;
        this.ItemID = id;
        this.quantity = quantity;

        this.InstanceID = System.Guid.NewGuid().ToString();
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

        OnBackpackModified += (i) => { PrintBackpack(); Clean();  };
    }

    #region Backpack

    private void Clean()
    {
        // Loop through each dictionary in the Backpack list
        foreach (var dict in Backpack)
        {
            // Collect all keys that should be removed
            var keysToRemove = new List<string>();

            foreach (var pair in dict)
            {
                InventoryItem item = pair.Value;

                // Handle null or destroyed references
                if (item == null || item.Equals(null) || item.quantity <= 0)
                    keysToRemove.Add(pair.Key);
            }

            // Remove them safely after iteration
            foreach (var key in keysToRemove)
                dict.Remove(key);
        }
    }

    private void BackpackInitialize()
    {
        for (int i = 0; i < ItemCategory.Count.ToInt(); i++)
        {
            Backpack.Add(new Dictionary<string, InventoryItem>());
        }

        var data = SaveSystem.Load_Inventory();
        if (data != null && data.Validate())
        {
            for (int i = 0; i < data.itemIDs.Count; i++)
            {
                var id = data.itemIDs[i]; var quantity = data.itemQuantities[i];

                GiveItem(id, quantity);
            }
        }
    }

    public bool UseItem(BaseItem item, int quantity)
    {
        if (quantity == 0) return true;

        var bp = GetBackpack(item);

        if (HasItem(item, quantity))
        {
            bp[item.ItemID].quantity -= quantity;
            OnBackpackModified?.Invoke(bp[item.ItemID]);
            return true;
        } else
        {
            return false;
        }
    }

    public bool HasItem(BaseItem item, int quantity)
    {
        if (quantity == 0) return true;

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

    public bool UseItem(string id, int amount)
    {
        if (amount == 0) return true;
        if (HasItem(id, amount))
        {
            // Search across all categories
            for (int i = 0; i < ItemCategory.Count.ToInt(); i++)
            {
                var bp = Backpack[i];
                if (bp.ContainsKey(id))
                {
                    bp[id].quantity -= amount;
                    return true;
                }
            }
        }

        return false;
    }

    public bool HasItem(string id, int amount)
    {
        if (amount == 0) return true;
        int totalCount = 0;

        // Search across all categories
        for (int i = 0; i < ItemCategory.Count.ToInt(); i++)
        {
            var bp = Backpack[i];
            if (bp.ContainsKey(id))
            {
                totalCount += bp[id].quantity;
            }
        }

        return totalCount >= amount;
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

    public void GiveItem(string itemID, int quantity)
    {
        if (quantity == 0) return;
        var item = MainDatabase.Instance.FindItem(itemID);

        if (item == null) return;

        GiveItem(item, quantity);
    }

    public void GiveItem(BaseItem item, int quantity)
    {
        if (quantity == 0) return;

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

    public Dictionary<string, InventoryItem> GetBackpack(BaseItem item)
    {
        if (!item) return null;

        return GetBackpack(item.ItemType);
    }

    public Dictionary<string, InventoryItem> GetBackpack(ItemCategory category)
    {
        return Backpack[category.ToInt()];
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

    private void OnDisable()
    {
        Save();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) Save();
    }

    private void Save()
    {
        List<InventoryItem> allItems = Backpack
            .SelectMany(dict => dict.Values)
            .ToList();

        SaveSystem.SaveInventory(allItems);
    }

    #endregion backpack
}
