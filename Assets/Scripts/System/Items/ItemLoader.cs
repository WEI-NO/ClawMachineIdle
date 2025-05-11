using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ItemLoader : MonoBehaviour
{
    [Header("Database Properties")]
    private Dictionary<ItemCategory, Dictionary<string, BaseItem>> database = new Dictionary<ItemCategory, Dictionary<string, BaseItem>>();

    private void Awake()
    {
        InitializeDatabase();
        LoadCategory(ItemCategory.Currency, (list) => { print("Preloaded Currency"); });
    }

    private void InitializeDatabase()
    {
        foreach (ItemCategory type in Enum.GetValues(typeof(ItemCategory)))
        {
            database[type] = new Dictionary<string, BaseItem>();
        }
    }

    /// <summary>
    /// Loads an item by its name using Addressables and stores it in the database.
    /// </summary>
    /// <param name="itemName">Name of the item to load.</param>
    /// <param name="onItemLoaded">Callback when the item is loaded.</param>
    public void LoadItem(string itemName, Action<BaseItem> onItemLoaded)
    {
        Addressables.LoadAssetAsync<BaseItem>(itemName).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                BaseItem item = handle.Result;
                if (item != null)
                {
                    AddItemToDatabase(item);
                    onItemLoaded?.Invoke(item);
                }
                else
                {
                    Debug.LogWarning($"Item '{itemName}' not found.");
                    onItemLoaded?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"Failed to load item: {itemName}");
                onItemLoaded?.Invoke(null);
            }
        };
    }

    /// <summary>
    /// Adds the loaded item to the corresponding dictionary in the database.
    /// </summary>
    /// <param name="item">The loaded BaseItem.</param>
    private void AddItemToDatabase(BaseItem item)
    {
        if (item == null) return;

        ItemCategory category = item.ItemType;
        string itemName = item.ItemName;

        if (!database.ContainsKey(category))
        {
            database[category] = new Dictionary<string, BaseItem>();
        }

        if (!database[category].ContainsKey(itemName))
        {
            database[category][itemName] = item;
            Debug.Log($"Item '{itemName}' added to the database under '{category}' category.");
        }
    }

    /// <summary>
    /// Retrieves a loaded item from the database.
    /// </summary>
    /// <param name="itemName">Name of the item.</param>
    /// <param name="category">Item category.</param>
    /// <returns>BaseItem or null if not found.</returns>
    public BaseItem GetItem(string itemName, ItemCategory category)
    {
        if (database.ContainsKey(category) && database[category].ContainsKey(itemName))
        {
            return database[category][itemName];
        }

        Debug.LogWarning($"Item '{itemName}' not found in '{category}' category.");
        return null;
    }

    /// <summary>
    /// Loads all items in a specific category using Addressables and stores them in the database.
    /// </summary>
    /// <param name="category">The category to load.</param>
    /// <param name="onCategoryLoaded">Callback when all items in the category are loaded.</param>
    public void LoadCategory(ItemCategory category, Action<List<BaseItem>> onCategoryLoaded)
    {
        string label = category.ToString();  // Label must match enum string
        List<BaseItem> loadedItems = new List<BaseItem>();

        Addressables.LoadAssetsAsync<BaseItem>(label, null).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (BaseItem item in handle.Result)
                {
                    if (item != null)
                    {
                        AddItemToDatabase(item);
                        loadedItems.Add(item);
                    }
                }

                Debug.Log($"Loaded {loadedItems.Count} items in category '{category}'.");
                onCategoryLoaded?.Invoke(loadedItems);
            }
            else
            {
                Debug.LogError($"Failed to load category: {category}");
                onCategoryLoaded?.Invoke(null);
            }
        };
    }

}