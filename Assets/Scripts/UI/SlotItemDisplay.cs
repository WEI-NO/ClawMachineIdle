using CustomLibrary.References;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotItemDisplay : MonoBehaviour
{
    public static SlotItemDisplay Instance;
    [Header("Components")]
    // Description
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI itemTitle;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image itemIcon;

    [Header("Default Settings")]
    [SerializeField] private string defaultTitle;
    [SerializeField] private string defaultDescription;
    [SerializeField] private int defaultCountText;

    private const float normalizedIconSize = 16.0f;

    private void Awake()
    {
        Initializer.SetInstance(this);
        DisplayItem(null);
    }

    public void DisplayItem(InventoryItem item)
    {
        var i = item != null ? item.item : null;
        descriptionText.text = i ? i.ItemDescription : defaultDescription;
        itemTitle.text = i ? i.ItemName : defaultTitle;
        countText.text = i ? item.quantity.ToString() : defaultCountText.ToString();

        if (i)
            SetSprite(itemIcon, i.ItemIcon, normalizedIconSize);
        else
            SetSprite(null, null);
        //itemIcon.sprite = i ? i.ItemIcon : null;
        //if (i)
        //{
        //    itemIcon.color = new Color(1f, 1f, 1f, 1f);
        //   itemIcon.rectTransform.sizeDelta = new Vector2(i.ItemIcon.rect.width, i.ItemIcon.rect.height);
        //} else
        //{
        //    itemIcon.color = new Color(1f, 1f, 1f, 0f);
        //}
    }

    public void SetSprite(Image img, Sprite sprite, float normalizedScale = normalizedIconSize)
    {
        if (img == null || sprite == null)
        {
            if (img) img.color = new Color(img.color.r, img.color.g, img.color.b, 0.0f);
            return;
        }

        var spriteDimension = new Vector2(sprite.rect.width, sprite.rect.height);
        var ratio = spriteDimension.y / spriteDimension.x;
        var w = normalizedScale;
        var h = w * ratio;

        img.sprite = sprite;
        img.rectTransform.sizeDelta = new Vector2(w, h);
        img.color = new Color(img.color.r, img.color.g, img.color.b, 1.0f);
    }

}
