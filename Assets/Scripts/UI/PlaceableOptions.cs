using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlaceableOptions : MonoBehaviour
{
    [Header("Components")]
    public List<PlaceableOptions> OptionButtons;
    private TouchController tc;
    private Animator anim;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI suffixText;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        if (tc = TouchController.Instance)
        {
            tc.OnSelectedBuildingChange += OnSelectedBuildingChange;
        }
    }

    private void OnSelectedBuildingChange(IsometricBuilding b)
    {
        if (!b)
        {
            // Hide
            anim.SetTrigger("Hide");
        } else
        {
            // Show
            anim.SetTrigger("Show");
            if (nameText)
            {
                nameText.text = b.BuildingName;
            }
            if (suffixText)
            {
                suffixText.text = b.Suffix;
            }

        }
    }

    private IsometricBuilding GetCurrentBuilding()
    {
        if (TouchController.Instance is var tc)
        {
            var selectedPlaceable = tc.selectedBuilding;
            if (selectedPlaceable)
            {
                return selectedPlaceable;
            }
        }

        return null;
    }

    #region Functionalities

    public void Info()
    {
        if (GetCurrentBuilding() is var placeable)
        {
            // Perform function
        }
    }

    public void ToStorage()
    {
        if (GetCurrentBuilding() is var placeable)
        {
            // Perform function
        }
    }

    public void Rotate()
    {
        if (GetCurrentBuilding() is var placeable)
        {
            // Perform function
        }
    }

    #endregion functionalities
}
