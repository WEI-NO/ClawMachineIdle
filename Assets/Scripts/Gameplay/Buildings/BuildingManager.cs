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
    public Vector2Int PlaceablePositions_Debug;
    public bool Add;


    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        var roomData = SaveSystem.Load_Room();
        int positionLength = roomData.buildingGridPositions.Count;
        int orientationLength = roomData.buildingOrientations.Count;
        for (int i = 0; i < roomData.buildingIDs.Count; i++)
        {
            if (i >= positionLength || i < 0)
            {
                Debug.LogWarning($"Building Manager: Load Failed, data {i} does not have a corresponding position");
                break;
            }

            if (i >= orientationLength)
            {
                Debug.LogWarning($"Building Manager: Load Failed, data {i} does not have a corresponding orientation");
                break;
            }

            var pos = roomData.buildingGridPositions[i];
            AddPlaceable(roomData.buildingIDs[i], new Vector2Int(pos.x, pos.y), roomData.buildingOrientations[i]);
        }
    }

    private void Update()
    {
        if (Add)
        {
            foreach (var i in StartingPlaceable_Debug)
            {
                AddPlaceable(i, PlaceablePositions_Debug, Orientation.Right);
            }
            Add = false;
        }
    }

    public void AddPlaceable(string buildingID)
    {
        AddPlaceable(buildingID, new Vector2Int(0, 0), Orientation.Right);
    }

    public void AddPlaceable(string buildingID, Vector2Int gridPosition, Orientation orientation)
    {
        PlaceableInfo newPlaceable = new PlaceableInfo(buildingID);
        if (newPlaceable != null)
        {
            StartCoroutine(newPlaceable.LoadAsset((x) =>
            {
                if (x)
                {
                    newPlaceable.inWorldObject = Instantiate(x, transform.position, Quaternion.identity);
                    newPlaceable.inWorldObject.SetFlip(orientation);
                    newPlaceable.inWorldObject.transform.SetParent(transform, true);
                    newPlaceable.inWorldObject.PlaceOnGridPosition(gridPosition);
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


    private void OnDisable()
    {
        SaveSystem.SaveRoom(PlaceableInformation);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveSystem.SaveRoom(PlaceableInformation);
        }
    }

    #region Save/Load



    #endregion save/load
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
            if (data == null)
            {
                Debug.LogWarning($"Can not find placeable: {buildingID}");
            }
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
}
