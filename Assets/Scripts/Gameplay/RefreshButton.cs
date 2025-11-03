using UnityEngine;
using TMPro;


public class RefreshButton : InGameButton
{
    public PrizeDumper prizeDumper;
    public TextMeshProUGUI costText;

    protected void Awake()
    {
        base.Awake();
        costText.text = $"{ClawMachineGameplay.Instance.CurrentCost()}";
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
        int refreshCost = ClawMachineGameplay.Instance.CurrentCost();
        string coinID = "currency001";
        if (PlayerInventory.Instance.UseItem(coinID, refreshCost))
        {
            ClawMachineGameplay.Instance.IncrementCost();
            costText.text = $"{ClawMachineGameplay.Instance.CurrentCost()}";
            prizeDumper.StartRefreshPrize();
        } else
        {
            return;
        }
    }
}