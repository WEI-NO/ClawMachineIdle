using CustomLibrary.References;
using UnityEngine;

public class ClawMachineGameplay : MonoBehaviour
{
    public static ClawMachineGameplay Instance;

    [Header("Refresh")]
    public int baseRefreshCost;
    public int refreshCostInterval = 50;
    public int currentStep = 0;
    public int maxRefreshCost;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    public void IncrementCost()
    {
        currentStep++;
    }

    public int CurrentCost()
    {
        return Mathf.Clamp(baseRefreshCost + currentStep * refreshCostInterval, baseRefreshCost, maxRefreshCost);
    }
}
