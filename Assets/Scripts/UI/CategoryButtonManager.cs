using System.Collections.Generic;
using System.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryButtonManager : MonoBehaviour
{
    [SerializeField] private List<BagCategoryButton> buttons;
    [SerializeField] private List<string> categoryNames;
    [SerializeField] private int currentSelectionIndex = 0;

    [SerializeField] private float defaultYOffset = 0f;
    [SerializeField] private float selectedYOffset = -20f;
    //[SerializeField] private Transform foregroundParent;

    [SerializeField] private TextMeshProUGUI categoryText;
    private BagUIContent bagContent;

    private void Start()
    {
        int j = 0;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).GetComponent<BagCategoryButton>() is var button)
            {
                button.buttonID = j++;
                buttons.Add(button);
            }
        }
        bagContent = GetComponentInParent<BagUIContent>();
        Choose(0);
    }

    public void Choose(int index)
    {
        if (index >= buttons.Count)
        {
            return;
        }

        var activeButton = buttons[index];
        currentSelectionIndex = index;
        foreach (var b in buttons)
        {
            float yOffset = defaultYOffset;
            // If the button is found
            if (b == activeButton)
            {
                
                b.SetActive(true);
                yOffset = selectedYOffset;
                SetCategoryName(index);
                if (bagContent)
                {
                    bagContent.ShowCategory((BagCategory)index);
                }
                //b.transform.SetParent(foregroundParent, true);
            }
            // If it is not the active button
            else
            {
                // And it is also active
                if (b.active)
                {
                    b.SetActive(false);
                    //b.transform.SetParent(transform, true);
                    yOffset = defaultYOffset;
                    //// Set sibling index so higher buttonID means higher in the UI
                    //int siblingIndex = buttons.Count - 1 - b.buttonID;
                    //b.transform.SetSiblingIndex(siblingIndex);
                }
            }

            RectTransform rect = b.transform as RectTransform;
            var pos = (b.transform as RectTransform).localPosition;
            rect.localPosition = new Vector3(pos.x, yOffset, pos.z);
        }
    }

    private void SetCategoryName(int categoryIndex)
    {
        string categoryName = "";
        switch (categoryIndex)
        {
            case 0:
                categoryName += "All";
                break;
            case 1:
                categoryName += "Plushies";
                break;
            case 2:
                categoryName += "Furnitures";
                break;
            case 3:
                categoryName += "WIP";
                break;
        }

        if (categoryText)
        {
            categoryText.text = categoryName;
        }
    }

    private void OnEnable()
    {
        Choose(0);
    }


}
