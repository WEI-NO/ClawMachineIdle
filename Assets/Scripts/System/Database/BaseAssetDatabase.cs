using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class BaseAssetDatabase<TData, TAsset> : ScriptableObject 
    where TData : ScriptableObject
    where TAsset : class
{
    [Header("All Data Entries")]
    public List<TData> allData;

    // Lookup dictionaries
    protected Dictionary<string, TData> idToData = new Dictionary<string, TData>();
    protected Dictionary<string, TAsset> loadedAssets = new Dictionary<string, TAsset>();

    // New: Rarity lookup (populated if TData supports rarity)
    protected Dictionary<ItemRarity, List<TData>> rarityToData = new Dictionary<ItemRarity, List<TData>>();

    // --- ABSTRACT: Must provide a way to get ID, address, and rarity from TData ---
    protected abstract string GetName(TData data);
    protected abstract string GetAddress(TData data);
    protected abstract string GetID(TData data);
    protected abstract ItemRarity GetRarity(TData data); // <-- Add this!

    // --- Initialization ---
    protected virtual void OnEnable()
    {
        idToData.Clear();
        rarityToData.Clear();

        if (allData != null)
        {
            foreach (var entry in allData)
            {
                if (entry != null)
                {
                    idToData[GetID(entry)] = entry;

                    // Add to rarity dictionary
                    ItemRarity rarity = GetRarity(entry);
                    if (!rarityToData.TryGetValue(rarity, out var list))
                    {
                        list = new List<TData>();
                        rarityToData[rarity] = list;
                    }
                    list.Add(entry);
                }
            }
        }
        Debug.Log("Initializing Database: " + this.name);
    }

    // --- Data Lookup ---
    public TData GetDataByID(string id)
    {
        idToData.TryGetValue(id, out var data);
        return data;
    }

    // --- Lookup all data by rarity ---
    public List<TData> GetAllDataByRarity(ItemRarity rarity)
    {
        if (rarityToData.TryGetValue(rarity, out var list))
            return list;
        return new List<TData>(); // Return empty list if none found
    }

    public IEnumerator LoadAssetCoroutine(TData data, Action<TAsset> onLoaded = null)
    {
        if (data == null)
        {
            onLoaded?.Invoke(null);
            yield break;
        }


        string id = GetName(data);

        // Notify request
        OnAssetRequested(data);

        // Check if asset is already loaded
        if (loadedAssets.TryGetValue(id, out var cachedAsset) && cachedAsset != null)
        {
            onLoaded?.Invoke(cachedAsset);
            yield break;
        }


        // Load by address
        string address = GetAddress(data);
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogWarning($"AssetDatabase: No address for {id}");
            onLoaded?.Invoke(null);
            yield break;
        }

        var handle = Addressables.LoadAssetAsync<GameObject>(address);
        yield return handle;


        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var asset = handle.Result.GetComponent<TAsset>();
            loadedAssets[id] = asset;
            OnAssetLoaded(data, asset); // <-- Notification
            onLoaded?.Invoke(asset);
        }
        else
        {
            Debug.LogError($"AssetDatabase: Failed to load asset at {address} (ID {id})");
            onLoaded?.Invoke(null);
        }
    }

    // --- Async Asset Loading (unchanged) ---
    //public async Task<TAsset> LoadAssetAsync(string id)
    //{
    //    if (loadedAssets.TryGetValue(id, out var asset) && asset != null)
    //        return asset;

    //    var data = GetDataByID(id);
    //    if (data == null)
    //    {
    //        Debug.LogWarning($"AssetDatabase: No data found for ID {id}");
    //        return default;
    //    }

    //    string address = GetAddress(data);
    //    if (string.IsNullOrEmpty(address))
    //    {
    //        Debug.LogWarning($"AssetDatabase: No address for ID {id}");
    //        return default;
    //    }

    //    var handle = Addressables.LoadAssetAsync<TAsset>(address);
    //    await handle.Task;

    //    if (handle.Status == AsyncOperationStatus.Succeeded)
    //    {
    //        loadedAssets[id] = handle.Result;
    //        return handle.Result;
    //    }
    //    else
    //    {
    //        Debug.LogError($"AssetDatabase: Failed to load asset at {address} (ID {id})");
    //        return default;
    //    }
    //}



    // --- Unload Asset (unchanged) ---
    public void UnloadAsset(string id)
    {
        if (loadedAssets.TryGetValue(id, out var asset) && asset != null)
        {
            Addressables.Release(asset);
            loadedAssets.Remove(id);
        }
    }

    // --- Bulk Unload (unchanged) ---
    public void UnloadAllAssets()
    {
        foreach (var pair in loadedAssets)
        {
            Addressables.Release(pair.Value);
        }
        loadedAssets.Clear();
    }

    // Random item of a certain rarity:
    public virtual TData GetRandomItemByRarity(ItemRarity rarity)
    {
        var list = GetAllDataByRarity(rarity);
        if (list.Count == 0) return null;
        int idx = UnityEngine.Random.Range(0, list.Count);
        return list[idx];
    }

    #region Listeners

    // Called whenever a new asset is successfully loaded for the first time
    protected virtual void OnAssetLoaded(TData data, TAsset asset) { }

    // Called whenever a load is requested (even if already loaded)
    protected virtual void OnAssetRequested(TData data) { }

    #endregion listeners
}
