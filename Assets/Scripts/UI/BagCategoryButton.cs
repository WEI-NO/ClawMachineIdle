using UnityEngine;
using UnityEngine.UI;

public class BagCategoryButton : MonoBehaviour
{
    public bool active = false;
    private Image icon;
    [SerializeField] private Image buttonImage;
    [SerializeField] Sprite activeSprite;
    [SerializeField] Sprite deactiveSprite;

    public int buttonID = 0;

    private void Start()
    {
        
    }

    public void InitializeCategoryButton(Sprite icon)
    {
        if (icon)
        {
            this.icon.sprite = icon;
            this.icon.rectTransform.sizeDelta = new Vector2(icon.rect.width, icon.rect.height);
        }
    }

    public void SetActive(bool state)
    {
        active = state;

        if (active)
        {
            buttonImage.sprite = activeSprite;
        } else
        {
            buttonImage.sprite = deactiveSprite;
        }
    }
}
