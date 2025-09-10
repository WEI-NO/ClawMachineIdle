using CustomLibrary.References;
using UnityEngine;
using UnityEngine.UI;

public class EggPopup : MonoBehaviour
{
    public static EggPopup Instance;

    [Header("References")]
    private Animator anim;
    [SerializeField] private Image prizeIcon;
    private BaseItem currentPrize;
    private int prizeQuantity;


    private void Awake()
    {
        Initializer.SetInstance(this);

        anim = GetComponent<Animator>();
    }

    // Should normally pass in an Egg
    public void StartView(BasePrize prizeToClaim)
    {
        if (prizeToClaim == null) return;

        currentPrize = prizeToClaim.RewardItem;
        Sprite prizeSprite = currentPrize.ItemIcon;
        prizeIcon.sprite = prizeSprite;
        prizeQuantity = prizeToClaim.Quantity;

        // Animation
        if (anim)
        {
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

        PlayerInventory.Instance.GiveItem(currentPrize, prizeQuantity);
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

        IncubationController.Instance.AddToQueue(currentPrize);
    }

}
