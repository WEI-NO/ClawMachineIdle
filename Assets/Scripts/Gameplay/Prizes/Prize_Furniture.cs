using UnityEngine;

public class Prize_Furniture : BasePrize
{
    public override void PrizeClaimFunction()
    {
        BaseItem furniture = null;
        var db = MainDatabase.Instance.DB_Placeable;

        if (db.GetRandomBuilding(ItemRarity) is var b)
        {
            furniture = b;
        }

        if (furniture)
        {
            PlayerInventory.Instance.GiveItem(furniture, 1);
        }
    }
}
