using CustomLibrary.References;
using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;

public class ClawMachineGameplay : MonoBehaviour
{
    public static ClawMachineGameplay Instance;

    public int baseCost = 100;

    [Header("Grab Settings")]
    public bool freeGrab = true;
    


    [Header("UI")]
    public TextMeshProUGUI refreshCostText;
    public TextMeshProUGUI grabCostText;

    [SerializeField] ClawObject clawBody;
    [SerializeField] PrizeDumper prizeDumper;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        clawBody = ClawObject.Instance;
            UpdateText();

    }

    public void StartGrabSequence()
    {
        if (clawBody == null) return;

        if (clawBody.InGrabSequence() || prizeDumper.InSequence)
            return;

        string coinID = "currency001";
        if (freeGrab)
        {
            grabCostText.text = $"Free";
            clawBody.StartGrabSequence();
            freeGrab = false;
        }
        else if (PlayerInventory.Instance.UseItem(coinID, baseCost))
        {
            clawBody.StartGrabSequence();
        }
        UpdateText();
    }

    public void StartRefreshSequence()
    {
        if (prizeDumper.InSequence || clawBody.InGrabSequence())
            return;

        string coinID = "currency001";
        if (PlayerInventory.Instance.UseItem(coinID, baseCost))
        {
            prizeDumper.StartRefreshPrize();
            freeGrab = true;
        }
        UpdateText();
    }

    public void UpdateText()
    {
        if (freeGrab)
        {
            grabCostText.text = "Free";
        } else
        {
            grabCostText.text = $"{baseCost}";
        }

        refreshCostText.text = $"{baseCost}";

    }
}
