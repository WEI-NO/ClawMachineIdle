using CustomLibrary.References;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EggPopup : MonoBehaviour
{
    public static EggPopup Instance;

    [Header("References")]
    private Animator anim;
    [SerializeField] private Image prizeIcon;
    [SerializeField] private TextMeshProUGUI itemCounter;
    private List<BaseItem> currentPrize = new List<BaseItem>();
    private List<int> prizeQuantity = new List<int>();
    private bool viewIsEnabled = false;
    private int prizeIndex = 0;
    private int totalPrizes = 0;
    private int currentIndex = 0;


    private void Awake()
    {
        Initializer.SetInstance(this);

        anim = GetComponent<Animator>();
    }

    public void AddPrize(BasePrize prizeToClaim)
    {
        if (prizeToClaim == null) return;

        currentPrize.Add(prizeToClaim.RewardItem);
        prizeQuantity.Add(prizeToClaim.Quantity);

        totalPrizes++;
        itemCounter.text = $"{currentIndex}/{totalPrizes}";
        if (!viewIsEnabled)
        {
            StartView();
        }
    }

    // Should normally pass in an Egg
    public void StartView()
    {
        if (currentPrize.Count <= 0)
        {
            totalPrizes = 0;
            currentIndex = 0;
            prizeIndex = 0;
            viewIsEnabled = false;
            return;
        }


        viewIsEnabled = true;
        currentIndex++;

        itemCounter.text = $"{currentIndex}/{totalPrizes}";

        Sprite prizeSprite = currentPrize[prizeIndex].ItemIcon;
        prizeIcon.sprite = prizeSprite;
        // Animation
        if (anim)
        {
            anim.ResetTrigger("End");
            viewIsEnabled = true;
            anim.SetTrigger("Start");
        }
    }

    public void SendToCollection()
    {
        // Animation
        if (anim)
        {
            anim.SetTrigger("End");
        }

        if (currentPrize == null)
        {
            return;
        }

        PlayerInventory.Instance.GiveItem(currentPrize[prizeIndex], prizeQuantity[prizeIndex]);
        currentPrize.RemoveAt(prizeIndex);
        prizeQuantity.RemoveAt(prizeIndex);

        StartView();
    }

    public void AddToIncubator()
    {
        // Animation
        if (anim)
        {
            anim.SetTrigger("End");
        }

        if (currentPrize == null)
        {
            return;
        }

        IncubationController.Instance.AddToQueue(currentPrize[prizeIndex]);
        currentPrize.RemoveAt(prizeIndex);
        prizeQuantity.RemoveAt(prizeIndex);

        StartView();
    }

    private void EndSequence()
    {
        if (currentPrize.Count >= 1 && currentPrize[prizeIndex] != null)
        {
            StartView();
        }
    }

}
