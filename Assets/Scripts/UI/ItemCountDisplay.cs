using TMPro;
using UnityEngine;

public class ItemCountDisplay : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI _countText;

    [Header("Display Properties")]
    [SerializeField] private string _itemName;
    [SerializeField] private ItemCategory _category;

    private void Update()
    {
        UpdateCountText();
    }

    private void UpdateCountText()
    {
        if (!_countText || PlayerInventory.Instance == null) return;

        var itemCount = PlayerInventory.Instance.GetItemCount(_itemName, _category);
        _countText.text = $"{itemCount}";
    }

}
