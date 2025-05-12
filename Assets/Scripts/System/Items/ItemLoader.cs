using CustomLibrary.References;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ItemLoader : MonoBehaviour
{
    public static ItemLoader Instance;

    [Header("Database Properties")]
    private Dictionary<ItemCategory, Dictionary<string, BaseItem>> database = new Dictionary<ItemCategory, Dictionary<string, BaseItem>>();

    // Pending loads for each item
    private Dictionary<string, List<Action<BaseItem>>> pendingLoads = new Dictionary<string, List<Action<BaseItem>>>();

    private void Awake()
    {
        Initializer.SetInstance(this);
        InitializeDatabase();
        //LoadCategory(ItemCategory.Currency, (list) => { print("Preloaded Currency"); });
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
        // Check if item is already loaded in the database
        foreach (var categoryDict in database.Values)
        {
            if (categoryDict.ContainsKey(itemName))
            {
                Debug.Log($"Item '{itemName}' found in database.");
                onItemLoaded?.Invoke(categoryDict[itemName]);
                return;
            }
        }

        // Check if this item is already being loaded
        if (pendingLoads.ContainsKey(itemName))
        {
            Debug.Log($"Item '{itemName}' is already being loaded. Adding callback to pending list.");
            pendingLoads[itemName].Add(onItemLoaded);
            return;
        }

        // Initialize the pending load list for this item
        pendingLoads[itemName] = new List<Action<BaseItem>> { onItemLoaded };

        // Proceed with Addressables load
        Debug.Log($"Loading item '{itemName}' from Addressables...");
        Addressables.LoadAssetAsync<BaseItem>(itemName).Completed += handle =>
        {
            BaseItem item = null;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                item = handle.Result;

                if (item != null)
                {
                    AddItemToDatabase(item);
                    Debug.Log($"Item '{itemName}' successfully loaded.");
                }
                else
                {
                    Debug.LogWarning($"Item '{itemName}' not found in Addressables.");
                }
            }
            else
            {
                Debug.LogError($"Failed to load item: {itemName}");
            }

            // Execute all pending callbacks
            if (pendingLoads.ContainsKey(itemName))
            {
                foreach (var callback in pendingLoads[itemName])
                {
                    callback?.Invoke(item);
                }

                // Remove item from pending loads
                pendingLoads.Remove(itemName);
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
    public BaseItem GetItem(string itemName, ItemCategory category = ItemCategory.None)
    {
        // If category is specified, only search that category
        if (category != ItemCategory.None)
        {
            if (database.ContainsKey(category) && database[category].ContainsKey(itemName))
            {
                return database[category][itemName];
            }

            Debug.LogWarning($"Item '{itemName}' not found in '{category}' category.");
            return null;
        }

        // Search all categories if 'None' is specified
        foreach (var categoryDict in database.Values)
        {
            if (categoryDict.ContainsKey(itemName))
            {
                return categoryDict[itemName];
            }
        }

        Debug.LogWarning($"Item '{itemName}' not found in any category.");
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