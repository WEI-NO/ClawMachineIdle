using UnityEngine;

[CreateAssetMenu(fileName = "CurrencyDatabase", menuName = "Bubble Claw/Database/Currency DB")]
public class CurrencyDatabase : BaseAssetDatabase<BaseItem, object>
{
    protected override string GetAddress(BaseItem data)
    {
        return data.ItemAddress;
    }

    protected override string GetID(BaseItem data)
    {
        return data.ItemID;
    }

    protected override string GetName(BaseItem data)
    {
        return data.ItemName;
    }

    protected override ItemRarity GetRarity(BaseItem data)
    {
        return data.itemRarity;
    }
}
