using CustomLibrary.References;
using TMPro;
using UnityEngine;

public class ClawMachineGameplay : MonoBehaviour
{
    public static ClawMachineGameplay Instance;

    [Header("Refresh")]
    public int baseRefreshCost;
    public int refreshCostInterval = 50;
    public int currentStep = 0;
    public int maxRefreshCost;

    [Header("UI")]
    public TextMeshProUGUI costText;

    [SerializeField] ClawObject clawBody;
    [SerializeField] PrizeDumper prizeDumper;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        clawBody = ClawObject.Instance;
    }

    public void IncrementCost()
    {
        currentStep++;
    }

    public int CurrentCost()
    {
        return Mathf.Clamp(baseRefreshCost + currentStep * refreshCostInterval, baseRefreshCost, maxRefreshCost);
    }

    public void StartGrabSequence()
    {
        if (clawBody == null) return;

        clawBody.StartGrabSequence();
    }

    public void StartRefreshSequence()
    {
        int refreshCost = CurrentCost();
        string coinID = "currency001";
        if (PlayerInventory.Instance.UseItem(coinID, refreshCost))
        {
            IncrementCost();
            costText.text = $"{CurrentCost()}";
            prizeDumper.StartRefreshPrize();
        }
    }
}
