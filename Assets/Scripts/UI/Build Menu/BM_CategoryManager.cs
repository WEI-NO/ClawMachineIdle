using CustomLibrary.References;
using UnityEngine;
using UnityEngine.UI;

public enum BM_Category
{
    Furniture,
    Plushie,
    Cosmetics,
    Count
}
public class BM_CategoryManager : MonoBehaviour
{
    [Header("Components")]
    public BM_CategoryButton[] CategoryButtons = new BM_CategoryButton[BM_Category.Count.ToInt()];

    private void Awake()
    {
        int i = 0;
        foreach (Transform child in transform)
        {
            var button = child.GetComponent<BM_CategoryButton>();
            if (button)
            {
                button.Initialize(GetCategory(i, true));
                button.OnPressed += OnCategorySwitch;
                CategoryButtons[i] = button;
                i++;
            }
            if (i >= CategoryButtons.Length)
            {
                break;
            }
        }
    }

    private void Start()
    {
        OnCategorySwitch(0);
    }

    private void OnCategorySwitch(int category)
    {
        int index = GetCategory(category);

        for (int i = 0; i < CategoryButtons.Length; i++)
        {
            CategoryButtons[i].SetDisable(i != index);
        }
    }

    public int GetCategory(int localIndex)
    {
        int maxIndex = (int)BM_Category.Count - 1;   // last valid
        int index = (localIndex == 0) ? maxIndex : maxIndex - localIndex;

        // clamp just in case
        index = Mathf.Clamp(index, 0, maxIndex);

        return index;
    }

    public BM_Category GetCategory(int localIndex, bool convert = true)
    {
        return (BM_Category)GetCategory(localIndex);
    }
    
}
