using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class ItemCountDisplay : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private Animator anim;

    [Header("Display Properties")]
    [SerializeField] private string _itemName;
    [SerializeField] private ItemCategory _category;
    [SerializeField] private bool shown = true;
    public TouchController tc;
    

    private void Awake()
    {
        anim = GetComponent<Animator>();
        shown = true;
    }

    private void Update()
    {
        UpdateCountText();

        if (TouchController.Instance != null && tc == null)
        {
            tc = TouchController.Instance;
            tc.OnEditModeEnter += Hide;
            tc.OnEditModeExit += Show;
        }

        if (TouchController.Instance != tc)
        {
            tc = null;
        }

    }

    private void UpdateCountText()
    {
        if (!_countText || PlayerInventory.Instance == null) return;

        var itemCount = PlayerInventory.Instance.GetItemCount(_itemName, _category);
        _countText.text = $"{itemCount}";
    }

    public void TriggerJump()
    {
        if (anim)
        {
            anim.SetTrigger("Jump");
        }
    }

    #region Show/Hide

    public void Show()
    {
        anim.SetTrigger("Show");
        shown = true;
    }

    public void Hide()
    {
        anim.SetTrigger("Hide");
        shown = false;
    }


    #endregion show/hide

}
