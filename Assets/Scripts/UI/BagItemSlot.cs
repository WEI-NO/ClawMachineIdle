using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BagItemSlot : MonoBehaviour
{
    private static int MaximumPixelDimension = 25;

    [SerializeField] private InventoryItem heldItem;
    private BaseItem item;


    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI countText;
    public void SetItem(InventoryItem heldItem)
    {
        if (this.heldItem != null)
        {
            if (heldItem.ItemID == this.heldItem.ItemID)
            {
                // Udpate count if the same heldItem;
                UpdateSlotDisplay();
            } else
            {
                // Tried to set another item.
            }
        } else
        {
            this.heldItem = heldItem;
            item = heldItem.item;
            UpdateSlotDisplay();
        }
    }

    public void UpdateSlotDisplay()
    {
        if (heldItem == null && item != null) return;

        if (iconImage && item.ItemIcon)
        {
            iconImage.rectTransform.sizeDelta = CalculateIconSize();
            iconImage.sprite = item.ItemIcon;
        }
        if (countText) countText.text = heldItem.quantity.ToString();
    }

    private Vector2 CalculateIconSize()
    {
        Vector2 result = Vector2.zero;
        if (iconImage && item.ItemIcon)
        {
            result = new Vector2(item.ItemIcon.rect.width, item.ItemIcon.rect.height);
        }

        if (result.x > MaximumPixelDimension || result.y > MaximumPixelDimension)
        {
            // Calculate scale factor to fit within box
            float scale = MaximumPixelDimension / Mathf.Max(result.x, result.y);
            result *= scale;

            // (Optional) Round to integer pixels for pixel art crispness
            result.x = Mathf.Floor(result.x);
            result.y = Mathf.Floor(result.y);
        }

        return result;
    }

    public void OnClick()
    {
        BagUIContent.Instance.SelectSlot(this);
    }

    public InventoryItem GetHeldItem()
    {
        return heldItem;
    }

    public BaseItem GetItem()
    {
        return item;
    }
}
