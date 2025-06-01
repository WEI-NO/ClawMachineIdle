using UnityEngine;

public class Prize_Plushie : BasePrize
{
    public override void PrizeClaimFunction()
    {
        BaseItem plushie = null;
        var db = MainDatabase.Instance.DB_Placeable;

        if (db.GetRandomPlushie(ItemRarity) is var p)
        {
            plushie = p;
        }

        if (plushie)
        {
            PlayerInventory.Instance.GiveItem(plushie, 1);
        }
    }
}
