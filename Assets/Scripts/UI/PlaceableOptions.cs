using CustomLibrary.References;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlaceableOptions : MonoBehaviour
{
    public static PlaceableOptions Instance;

    [Header("Components")]
    public List<PlaceableOptions> OptionButtons;
    private TouchController tc;
    private Animator anim;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI suffixText;

    public Transform content;

    private IsometricBuilding lastSavedBuilding = null;

    private bool currentState = false;
    private void Awake()
    {
        Initializer.SetInstance(this);

        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        if (tc = TouchController.Instance)
        {
            //tc.OnSelectedBuildingChange += OnSelectedBuildingChange;
        }
    }

    private void Update()
    {
        //if (currentState && TouchController.Instance.EditMode)
        //{
        //    anim.SetTrigger("Hide");
        //    currentState = false;
        //}
    }

    public void SetBuilding(IsometricBuilding b)
    {
        if (b == lastSavedBuilding) return;
        ClearOptions();
        if (!b)
        {
            // Hide
            anim.SetTrigger("Hide");
            currentState = false;
        } else
        {
            if (b.GetComponent<OptionsCustomizer>() is OptionsCustomizer customizer)
            {
                foreach (var o in customizer.GetOptions())
                {
                    var newO = Instantiate(o, content);
                    newO.Initialize(b);
                }
            }

            // Show
            anim.SetTrigger("Show");
            currentState = true;
            if (nameText)
            {
                nameText.text = b.BuildingName;
            }
            if (suffixText)
            {
                suffixText.text = b.Suffix;
            }
        }
        lastSavedBuilding = b;
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

    private void ClearOptions()
    {
        foreach (Transform t in content)
        {
            Destroy(t.gameObject);
        }
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
            if (MainDatabase.Instance.DB_Placeable.GetDataByID(placeable.BuildingID) is var data)
            {
                PlayerInventory.Instance.GiveItem(data, 1);
                Destroy(placeable.gameObject);
                SetBuilding(null);
            }
        }
    }

    public void Rotate()
    {
        if (GetCurrentBuilding() is var placeable)
        {
            // Perform function
            placeable.Flip();
        }
    }

    #endregion functionalities
}
