using UnityEngine;
using UnityEngine.UI;

public class QueueSlotUI : MonoBehaviour
{
    [Header("Components")]
    private Image slotImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image timerIndicatorImage;
    [SerializeField] private Image claimableImage;


    [Header("Sprites")]
    public Sprite lockedSprite;
    public Sprite normalSprite;

    [Header("Slot Settings")]
    public int slotOrder = 0;
    public bool locked = false;
    public EggContainer displayingContainer;
    public bool hasContainer = false;

    private void Awake()
    {
        slotImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (hasContainer)
        {
            if (displayingContainer == null) { hasContainer = false; return; }

            // Timer Indicator
            if (!displayingContainer.Done())
            {
                claimableImage.gameObject.SetActive(false);
                timerIndicatorImage.gameObject.SetActive(true);
                float fillAmount = displayingContainer.currentHatchTime / displayingContainer.hatchTime;
                timerIndicatorImage.fillAmount = fillAmount;
            } else
            {
                claimableImage.gameObject.SetActive(true);
                timerIndicatorImage.gameObject.SetActive(false);
            }


            iconImage.gameObject.SetActive(true);
            iconImage.sprite = displayingContainer.heldEgg.ItemIcon;
            // Update Timer as well
        } else
        {
            iconImage.gameObject.SetActive(false);
            claimableImage.gameObject.SetActive(false);
            timerIndicatorImage.gameObject.SetActive(false);
        }
    }

    public void SetContainer(EggContainer container)
    {
        displayingContainer = container;
        hasContainer = displayingContainer != null;
    }

    public void SetLockedState(bool state)
    {
        if (locked == state) return;

        if (state)
        {
            slotImage.sprite = lockedSprite;
            iconImage.gameObject.SetActive(false);
        }
        else
        {
            slotImage.sprite = normalSprite;
        }

        locked = state;
    }

}
