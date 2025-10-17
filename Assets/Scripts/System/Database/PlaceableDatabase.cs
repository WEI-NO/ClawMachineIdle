using System.Collections.Generic;
using UnityEngine;

public class PlaceableDatabase : BaseAssetDatabase<PlaceableItem, IsometricBuilding>
{
    // All plushies and all buildings
    public Dictionary<string, PlaceableItem> plushies { get; private set; } = new Dictionary<string, PlaceableItem>();
    public Dictionary<string, PlaceableItem> buildings { get; private set; } = new Dictionary<string, PlaceableItem>();

    // Optional: Rarity-sorted versions for even faster filtering
    public Dictionary<ItemRarity, List<PlaceableItem>> plushiesByRarity { get; private set; } = new();
    public Dictionary<ItemRarity, List<PlaceableItem>> buildingsByRarity { get; private set; } = new();

    protected override string GetAddress(PlaceableItem data) => data.ItemAddress;
    protected override string GetName(PlaceableItem data) => data.ItemName;
    protected override string GetID(PlaceableItem data) => data.ItemID;
    protected override ItemRarity GetRarity(PlaceableItem data) => data.itemRarity;

    // --- Override OnEnable to partition data ---
    protected override void OnEnable()
    {
        base.OnEnable(); // sets up rarityToData

        plushies.Clear();
        buildings.Clear();
        plushiesByRarity.Clear();
        buildingsByRarity.Clear();

        if (allData != null)
        {
            foreach (var item in allData)
            {
                if (item == null) continue;
                if (item.isPlushie)
                {
                    plushies.Add(item.ItemID, item);

                    // Add to plushiesByRarity
                    if (!plushiesByRarity.TryGetValue(item.itemRarity, out var list))
                    {
                        list = new List<PlaceableItem>();
                        plushiesByRarity[item.itemRarity] = list;
                    }
                    list.Add(item);
                }
                else
                {
                    buildings.Add(item.ItemID, item);

                    // Add to buildingsByRarity
                    if (!buildingsByRarity.TryGetValue(item.itemRarity, out var list))
                    {
                        list = new List<PlaceableItem>();
                        buildingsByRarity[item.itemRarity] = list;
                    }
                    list.Add(item);
                }
            }
        }
    }

    // --- Get random plushie/building by rarity ---
    public PlaceableItem GetRandomPlushie(ItemRarity rarity)
    {
        if (plushiesByRarity.TryGetValue(rarity, out var list) && list.Count > 0)
            return list[UnityEngine.Random.Range(0, list.Count)];
        return null;
    }

    public PlaceableItem GetRandomBuilding(ItemRarity rarity)
    {
        if (buildingsByRarity.TryGetValue(rarity, out var list) && list.Count > 0)
            return list[UnityEngine.Random.Range(0, list.Count)];
        return null;
    }

    // Optionally: All plushies/buildings of a rarity
    public List<PlaceableItem> GetPlushiesByRarity(ItemRarity rarity)
        => plushiesByRarity.TryGetValue(rarity, out var list) ? list : null;

    public List<PlaceableItem> GetBuildingsByRarity(ItemRarity rarity)
        => buildingsByRarity.TryGetValue(rarity, out var list) ? list : null;

    public override PlaceableItem GetDataByID(string id)
    {
        if (plushies.TryGetValue(id, out var p))
        {
            return p;
        }

        if (buildings.TryGetValue(id, out var b))
        {
            return b;
        }

        return null;
    }
}
