using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PrizeEntry
{
    public PrizeItem prize;
    public int entry;
}


[CreateAssetMenu(menuName = "Game/Prize List")]
public class SO_PrizeEntries : ScriptableObject
{
    public List<PrizeEntry> entries = new List<PrizeEntry>();
    public List<float> entryOdds = new List<float>();
    public bool oddCalculated = false;

    public PrizeEntry RollPrize()
    {
        CalculateOdds();

        float rarityRoll = UnityEngine.Random.Range(0.0f, 1.0f);
        int selectedIndex = 0;
        for (int i = entryOdds.Count - 1; i >= 0; i--)
        {
            if (rarityRoll >= (1.0f - entryOdds[i])) 
            {
                selectedIndex = i;
                break;
            }
        }
        if (selectedIndex < entries.Count)
        {
            return entries[selectedIndex];
        } else
        {
            return null;
        }
    }

    public void CalculateOdds()
    {
        //if (clean) return;
        if (entries == null)
        {
            return;
        }

        entryOdds = new List<float>();
        int sum = 0;
        foreach (var i in entries)
        {
            sum += i.entry;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (sum == 0)
            {
                entryOdds.Add(1.0f);
            }
            else
            {
                entryOdds.Add((float)entries[i].entry / sum);
            }
        }
        oddCalculated = true;
    }
}