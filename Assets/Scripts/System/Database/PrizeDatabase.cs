using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using CustomLibrary.References;

/// <summary>
/// Loads all addressables with the label "Prize" and stores them in a list.
/// </summary>
[CreateAssetMenu(fileName ="PrizeDatabase", menuName ="Bubble Claw/Database/PrizeDatabase")]
public class PrizeDatabase : BaseAssetDatabase<PrizeItem, BasePrize>
{
    protected override string GetID(PrizeItem data)
    {
        return data.ItemName;
    }

    protected override string GetAddress(PrizeItem data)
    {
        return data.ItemAddress;
    }

    protected override ItemRarity GetRarity(PrizeItem data)
    {
        return data.itemRarity;
    }
}   

//public static PrizeDatabase Instance;
     //public List<List<BasePrize>> Prizes { get; private set; } = new List<List<BasePrize>>();

//// Optionally: Event when finished loading
//public System.Action OnPrizeDatabaseLoaded;

//protected  void OnAwake()
//{
//    Initializer.SetInstance(this);
//    LoadAllPrizes();

//    OnPrizeDatabaseLoaded += Print;
//}

//public void Print()
//{
//    string result = "";

//    foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
//    {
//        int rarityIndex = rarity.ToInt();
//        if (rarity == ItemRarity.Count) continue;
//        result += $"{rarity.ToString()}: {Prizes[rarityIndex].Count}\n";
//    }

//    print(result);
//}

///// <summary>
///// Loads all assets labeled as "Prize" asynchronously.
///// </summary>
//public async void LoadAllPrizes()
//{
//    Prizes.Clear();

//    foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
//    {
//        if (rarity == ItemRarity.Count) continue;
//        Prizes.Add(new List<BasePrize>());
//    }

//    // Load all BasePrize assets with the "Prize" label
//    AsyncOperationHandle<IList<GameObject>> handle = Addressables.LoadAssetsAsync<GameObject>("Prize", null);

//    await handle.Task; // Await for completion

//    if (handle.Status == AsyncOperationStatus.Succeeded)
//    {
//        foreach (var prize in handle.Result)
//        {
//            if (prize)
//            {
//                BasePrize prizeBase = prize.GetComponent<BasePrize>();
//                if (prizeBase)
//                {
//                    int rarityIndex = prizeBase.ItemRarity.ToInt();
//                    Prizes[rarityIndex].Add(prizeBase);
//                }
//            }


//        }

//        Debug.Log($"[PrizeDatabase] Loaded {Prizes.Count} prizes.");
//        OnPrizeDatabaseLoaded?.Invoke();
//    }
//    else
//    {
//        Debug.LogError("[PrizeDatabase] Failed to load prizes with label 'Prize'.");
//    }
//}

//public BasePrize GetRandomPrize(ItemRarity rarity)
//{
//    if (!HasPrize(rarity)) return null;

//    int index = rarity.ToInt();
//    var rarityList = Prizes[index];
//    int randomIndex = UnityEngine.Random.Range(0, rarityList.Count);

//    BasePrize prize = Instantiate(Prizes[index][randomIndex].gameObject).GetComponent<BasePrize>();
//    return prize;
//}

//public bool HasPrize(ItemRarity rarity)
//{
//    return Prizes[rarity.ToInt()].Count > 0;
//}