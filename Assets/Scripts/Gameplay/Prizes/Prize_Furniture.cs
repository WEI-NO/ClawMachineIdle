using UnityEngine;

public class Prize_Furniture : BasePrize
{
    //public SO_EggPrizeEntries dropTable;

    protected override void OnStart()
    {
        RewardItem = MainDatabase.Instance.DB_Egg.GetDataByID(Reward);
    }

    public override void PrizeClaimFunction()
    {
        if (EggPopup.Instance == null)
        {
            DefaultPrizeClaim();
        } else
        {
            EggPopup.Instance.AddItem(this);
            PrizeClaimEffectSpawn();
        }
    }
}
