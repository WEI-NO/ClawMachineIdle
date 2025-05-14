using CustomLibrary.References;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class Spawner : GameModule
{
    public static Spawner Instance;

    [Header("References")]
    public BuildingSelector buildingSelector;

    [Header("Components")]
    public BuildingSpawner buildingSpawner;


    private void Awake()
    {
        Initializer.SetInstance(this);
        buildingSpawner = GetComponent<BuildingSpawner>();
    }

    private void Start()
    {
        buildingSelector = BuildingSelector.Instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnBuilding(0);
        }

        if (Input.GetKeyDown(KeyCode.Return) && buildingSelector.currentBuilding)
        {
            ConfirmBuilding();
        }
    }


    #region Building Spawner

    public void SpawnBuilding(int id)
    {
        var building = buildingSpawner.Get(id);
        if (building)
        {
            var newB = Instantiate(building);
            buildingSelector.SelectBuilding(newB);
        }
    }

    public void ConfirmBuilding()
    {
        buildingSelector.PlaceBuilding();
    }

    #endregion building spawner
}
