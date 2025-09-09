using UnityEngine;

public class Prize_Plushie : BasePrize
{
    protected override void OnStart()
    {
        RewardItem = MainDatabase.Instance.DB_Egg.GetDataByID(Reward);
    }

    public override void PrizeClaimFunction()
    {
        if (EggPopup.Instance == null)
        {
            base.PrizeClaimFunction();
        }
        else
        {
            EggPopup.Instance.StartView(this);
        }
    }
}
