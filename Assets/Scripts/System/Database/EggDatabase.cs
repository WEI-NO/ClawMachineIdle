using UnityEngine;

[CreateAssetMenu(fileName = "EggDatabase", menuName = "Bubble Claw/Database/Egg Database")]
public class EggDatabase : BaseAssetDatabase<EggItem, object>
{
    protected override string GetAddress(EggItem data)
    {
        return data.ItemAddress;
    }

    protected override string GetID(EggItem data)
    {
        return data.ItemID;
    }

    protected override string GetName(EggItem data)
    {
        return data.ItemName;
    }

    protected override ItemRarity GetRarity(EggItem data)
    {
        return data.itemRarity;
    }

}
