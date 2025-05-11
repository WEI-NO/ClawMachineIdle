using CustomLibrary.References;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    public BaseItem item;
    public string itemName;
    public int quantity;

    public InventoryItem(BaseItem item, string itemName, int quantity)
    {
        this.item = item;
        this.itemName = itemName;
        this.quantity = quantity;
    }
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    public List<Dictionary<string, InventoryItem>> Backpack = new List<Dictionary<string, InventoryItem>>();

    private void Awake()
    {
        Initializer.SetInstance(this);

        BackpackInitialize();
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
            bp[item.ItemName].quantity -= quantity;
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

        if (bp.ContainsKey(item.ItemName))
        {
            return quantity <= bp[item.ItemName].quantity;
        } else
        {
            return false;
        }
    }

    public void GiveItem(BaseItem item, int quantity)
    {
        var bp = GetBackpack(item);

        if (bp == null) return;

        if (bp.ContainsKey(item.ItemName))
        {
            bp[item.ItemName].quantity += quantity;
        }
        else
        {
            InventoryItem newItem = new InventoryItem(item, item.ItemName, quantity);
            bp.Add(item.ItemName, newItem);
        }
    }

    private Dictionary<string, InventoryItem> GetBackpack(BaseItem item)
    {
        if (!item) return null;

        return Backpack[item.ItemType.ToInt()];
    }

    #endregion backpack
}
