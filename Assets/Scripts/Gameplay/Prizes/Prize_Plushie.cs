using UnityEngine;

public class Prize_Plushie : BasePrize
{
    protected override void OnStart()
    {
        RewardItem = MainDatabase.Instance.DB_Egg.GetDataByID(Reward);
    }

    public override void PrizeClaimFunction()
    {
        base.PrizeClaimFunction();
    }
}
