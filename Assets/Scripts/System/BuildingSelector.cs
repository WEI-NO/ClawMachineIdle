using CustomLibrary.References;
using UnityEngine;

public class BuildingSelector : GameModule
{
    public static BuildingSelector Instance;

    protected override void OnAwake()
    {
        Initializer.SetInstance(this);
    }

    public BaseGridObject currentBuilding;

    public void SelectBuilding(BaseGridObject obj)
    {

        if (currentBuilding != null)
        {
            if (!currentBuilding.SetInWorld)
            {
                Destroy(currentBuilding.gameObject);
            } else
            {
                currentBuilding.ChangeState(GridObjectState.InWorld);
            }
            currentBuilding = null;
        }

        if (obj != null)
        {
            obj.ChangeState(GridObjectState.Moving);
            currentBuilding = obj;
        }
    }

    public void PlaceBuilding()
    {
        if (currentBuilding == null) return;

        currentBuilding.ChangeState(GridObjectState.InWorld);
        currentBuilding = null;

    }

}
