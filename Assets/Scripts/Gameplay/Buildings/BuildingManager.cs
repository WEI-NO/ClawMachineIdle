using System;
using UnityEngine;
using System.Collections.Generic;
using CustomLibrary.References;
using System.Collections;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance;

    [Header("Containers")]
    public List<PlaceableInfo> PlaceableInformation = new List<PlaceableInfo>();
    public List<string> StartingPlaceable_Debug = new List<string>();

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        foreach (var i in StartingPlaceable_Debug)
        {
            AddPlaceable(i);
        }
    }

    public void AddPlaceable(string buildingID)
    {
        PlaceableInfo newPlaceable = new PlaceableInfo(buildingID);
        if (newPlaceable != null)
        {
            StartCoroutine(newPlaceable.LoadAsset((x) =>
            {
                if (x)
                {
                    newPlaceable.inWorldObject = Instantiate(x, transform.position, Quaternion.identity);
                    newPlaceable.inWorldObject.PlaceOnGridPosition(new Vector2Int(10, 50));
                    newPlaceable.inWorldObject.OnPlaceableDestroy += RemovePlaceable;
                    PlaceableInformation.Add(newPlaceable);
                }
            }));
        }
    }

    public void RemovePlaceable(IsometricBuilding placeable)
    {
        var match = PlaceableInformation.FindIndex(info => info.inWorldObject == placeable);
        if (match >= 0 && match < PlaceableInformation.Count)
        {
            PlaceableInformation.RemoveAt(match);
        }
    }

}

[System.Serializable]
public class PlaceableInfo
{
    public bool assetLoaded = false;

    public IsometricBuilding inWorldObject;
    public PlaceableItem objectData;

    [Header("Save Data")]
    public Vector2Int pixelPosition;

    public PlaceableInfo(string buildingID)
    {
        assetLoaded = false;
        if (MainDatabase.Instance.DB_Placeable.GetDataByID(buildingID) is var data)
        {
            objectData = data;
        }
    }

    public IEnumerator LoadAsset(Action<IsometricBuilding> action)
    {
        if (objectData != null)
        {
            
            yield return MainDatabase.Instance.DB_Placeable.LoadAssetCoroutine(objectData, (x) =>
            {
                Debug.Log(x);
                if (x != null)
                {
                    assetLoaded = true;
                    action.Invoke(x);
                }
            });
        }
        yield return null;
    }

    public void SaveData()
    {
        if (inWorldObject != null)
        {
            pixelPosition = inWorldObject.blueprint.GridPosition;
        }
    }

    public bool Validate()
    {
        return inWorldObject != null;
    }
}
