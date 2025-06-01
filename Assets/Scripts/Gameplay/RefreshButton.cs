using UnityEngine;


public class RefreshButton : InGameButton
{
    public PrizeDumper prizeDumper;
    protected override void ButtonFunction()
    {
        prizeDumper.StartRefreshPrize();
    }
}