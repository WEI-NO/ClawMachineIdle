using UnityEngine;

public class Prize_Furniture : BasePrize
{
    public SO_EggPrizeEntries dropTable;

    public override void PrizeClaimFunction()
    {
        //if (dropTable)
        //{
        //    var db = MainDatabase.Instance.DB_Placeable;
        //    var prizeData = dropTable.RollPrize().prize;
            
        //    if (prizeData)
        //    {
        //        PlayerInventory.Instance.GiveItem(prizeData, 1);
        //    }
        //}
    }
}
