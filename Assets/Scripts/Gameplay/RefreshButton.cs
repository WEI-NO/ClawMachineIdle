using UnityEngine;
using TMPro;


public class RefreshButton : InGameButton
{
    public PrizeDumper prizeDumper;
    public TextMeshProUGUI costText;

    protected override void OnAwake()
    {
        costText.text = $"{ClawMachineGameplay.Instance.baseCost}";
    }

    private void Update()
    {
        if (ClawObject.Instance != null && ClawObject.Instance.inSequence)
        {
            animator.SetBool("Disabled", true);
        } else
        {
            animator.SetBool("Disabled", false);
        }
    }

    protected override void ButtonFunction()
    {
        if (animator.GetBool("Disabled"))
        {
            return;
        }

        int refreshCost = ClawMachineGameplay.Instance.baseCost;
        string coinID = "currency001";
        if (PlayerInventory.Instance.UseItem(coinID, refreshCost))
        {
            costText.text = $"{ClawMachineGameplay.Instance.baseCost}";
            prizeDumper.StartRefreshPrize();
        } else
        {
            return;
        }
    }
}