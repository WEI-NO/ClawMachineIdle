using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Items/Database")]
public abstract class BaseItemDatabase : ScriptableObject
{
    private static BaseItemDatabase Instance;

    public List<BaseItem> allItems;

    // (Optional) Quick lookup by ID
    private Dictionary<string, BaseItem> idLookup;
    private bool initialized = false;

    private void OnEnable()
    {
        if (!initialized)
            InitializeLookup();
    }

    private void InitializeLookup()
    {
        Instance = this;
        idLookup = new Dictionary<string, BaseItem>();
        foreach (var item in allItems)
        {
            if (item != null && !string.IsNullOrEmpty(item.ItemName))
                idLookup[item.ItemName] = item;
        }
        initialized = true;
        Debug.Log(idLookup.Count);
    }

    public static BaseItem GetItemByID(string id)
    {
        if (!Instance.initialized) Instance.InitializeLookup();
        Instance.idLookup.TryGetValue(id, out var item);
        return item;
    }

    // Add more utility methods as needed!
}