using CustomLibrary.References;
using UnityEngine;
using UnityEngine.UI;

public class EggPopup : MonoBehaviour
{
    public static EggPopup Instance;

    [Header("References")]
    private Animator anim;
    [SerializeField] private Image prizeIcon;
    private BasePrize currentPrize;


    private void Awake()
    {
        Initializer.SetInstance(this);

        anim = GetComponent<Animator>();
    }

    // Should normally pass in an Egg
    public void StartView(BasePrize prizeToClaim)
    {
        if (prizeToClaim == null) return;

        currentPrize = prizeToClaim;
        Sprite prizeSprite = currentPrize.RewardItem.ItemIcon;
        prizeIcon.sprite = prizeSprite;

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
    }

    public void AddToIncubator()
    {
        // Animation
        if (anim)
        {
            anim.SetTrigger("End");
        }

        if (IncubationController.Instance == null || currentPrize == null)
        {
            print("null or currentPrize is null");
            return;
        }

        IncubationController.Instance.AddToQueue(currentPrize.RewardItem);


    }

}
