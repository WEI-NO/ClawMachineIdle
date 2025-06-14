using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ItemEntry<T>
{
    public T prize;
    public int entry;
}


public class SO_BasePrizeEntries<T1, T2> : ScriptableObject where T1 : ItemEntry<T2>
{
    public List<T1> entries = new List<T1>();
    public List<float> entryOdds = new List<float>();
    public bool oddCalculated = false;

    public T1 RollPrize()
    {
        CalculateOdds();

        float rarityRoll = UnityEngine.Random.Range(0f, 1f);
        float cumulative = 0f;

        for (int i = 0; i < entryOdds.Count; i++)
        {
            cumulative += entryOdds[i];
            if (rarityRoll < cumulative)
            {
                return entries[i];
            }
        }

        // Fallback: return the last entry if not found due to float rounding issues
        if (entries.Count > 0)
            return entries[entries.Count - 1];

        return null;
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
