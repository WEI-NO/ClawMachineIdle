using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IncubatorDisplay : OpenableUI
{
    [Header("References")]
    public Image eggImage;
    public Sprite emptyEggSprite;
    public TextMeshProUGUI timeDisplay;
    public TextMeshProUGUI timeTitle;
    public IncubationController controller;

    [Header("Settings")]
    public bool updateHatchery = false;
    public string readyText = "READY!";
    public string hatchingText = "HATCHING IN:";


    private void Start()
    {
        controller = IncubationController.Instance;
    }

    private void Update()
    {
        if (updateHatchery)
        {
            var container = controller.GetFirstInQueue(false);
            if (container != null)
            {

                eggImage.sprite = container.heldEgg.ItemIcon;
                eggImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                // If the egg is still hatching
                if (!container.Done())
                {
                    timeTitle.text = hatchingText;
                    timeDisplay.text = CustomLibrary.Time.TimeFormatter.TimeToDisplay(container.hatchTime - container.currentHatchTime);
                    timeDisplay.gameObject.SetActive(true);
                    timeTitle.gameObject.SetActive(true);
                }
                else
                {
                    timeDisplay.gameObject.SetActive(false);
                    //timeTitle.gameObject.SetActive(false);
                    timeTitle.text = readyText;
                }
            }
            else
            {
                eggImage.sprite = emptyEggSprite;
                eggImage.color = new Color(0, 0, 0, 0.8f);
                timeDisplay.gameObject.SetActive(false);
                timeTitle.gameObject.SetActive(false);
            }
        }
    }

    protected override void ToggledOn()
    {
        updateHatchery = true;
    }

    protected override void ToggledOff()
    {
        updateHatchery = false;
    }


}
