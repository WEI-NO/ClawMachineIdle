using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemLoader : MonoBehaviour
{
    [Header("Database Properties")]
    private Dictionary<ItemCategory, Dictionary<string, BaseItem>> database;

    private void Awake()
    {
        // Initialize dictionary
        foreach (ItemCategory type in Enum.GetValues(typeof(ItemCategory)))
        {
            database[type] = new Dictionary<string, BaseItem>();
        }
    }

}
