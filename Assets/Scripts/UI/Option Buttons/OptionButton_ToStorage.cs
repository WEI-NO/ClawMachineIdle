using UnityEngine;

public class OptionButton_ToStorage : BaseOptionButton
{
    protected override void ActivateFunction()
    {
        if (target is var placeable)
        {
            // Perform function
            if (MainDatabase.Instance.DB_Placeable.GetDataByID(placeable.BuildingID) is var data)
            {
                PlayerInventory.Instance.GiveItem(data, 1);
                Destroy(placeable.gameObject);
                PlaceableOptions.Instance.SetBuilding(null);
            }
        }
    }
}
