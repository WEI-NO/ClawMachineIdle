using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuSlot : MonoBehaviour
{
    const float IconSizeNormalized = 100.0f;

    [Header("References")]
    public PlaceableItem assignedPlaceable;
    public int quantity = 0;

    [Header("Components (Visuals)")]
    public Image icon;
    public TextMeshProUGUI countText;

    [Header("Events")]
    public Action<BuildMenuSlot> OnDestroy;

    public void AssignBuilding(BaseItem item, int Quantity)
    {
        if (item == null) return;

        if (item is PlaceableItem pi)
        {
            assignedPlaceable = pi;
        }

        var sprite = assignedPlaceable.ItemIcon;
        float width = sprite.rect.width;
        float height = sprite.rect.height;
        float ratio = width / height;
        icon.sprite = sprite;
        icon.rectTransform.sizeDelta = new Vector2(IconSizeNormalized * ratio, IconSizeNormalized);
        quantity = Quantity;
        countText.text = $"{quantity}";
    }

    public void SpawnBuilding()
    {
        
        if (!PlayerInventory.Instance.UseItem(assignedPlaceable, 1))
        {
            return;
        }
        BuildingManager.Instance.AddPlaceable(assignedPlaceable.ItemID);
        quantity -= 1;

        countText.text = $"{quantity}";

        if (quantity <= 0)
        {
            Destroy(gameObject);
            OnDestroy?.Invoke(this);
        }
    }
}
