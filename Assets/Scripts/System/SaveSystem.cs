using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum SaveFile
{ 
    Base,
    Inventory
}


public class SaveSystem
{
    // Room Data
    private static RoomData roomData;
    private const string RoomDataFileName = "roomdata.json";

    #region Public Interface - Room Saving

    private static string FullPath(string filename)
    {
        return Path.Combine(Application.persistentDataPath, filename);
    }

    public static bool SaveRoom(List<PlaceableInfo> infos)
    {
        roomData = new RoomData();
        foreach (var i in infos)
        {
            roomData.AddData(i);
        }

        string path = FullPath(RoomDataFileName);

        try
        {
            var json = JsonUtility.ToJson(roomData);

            // To prevent crashes mid saving
            var tempPath = path + ".tmp";
            File.WriteAllText(tempPath, json);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tempPath, path);

            return true;
        } catch (System.Exception e)
        {
                Debug.LogError($"Save Syste: {e}");
        }

        return false;
    }

       
    public static RoomData Load()
    {
        // Creates a new save if nothing is found
        string path = FullPath(RoomDataFileName);

        try
        {
            if (!File.Exists(path))
                return new RoomData();

            var json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<RoomData>(json);
            return data ?? new RoomData();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Load failed, creating new data. Reason: {e}");
            return new RoomData();
        }

    }

    #endregion public interface - room saving
}


// Room Data

public class RoomData
{
    public List<string> buildingIDs = new List<string>();
    public List<Vector2Int_Save> buildingGridPositions = new List<Vector2Int_Save>();
    public List<Orientation> buildingOrientations = new List<Orientation>();

    public RoomData()
    {
        buildingIDs = new List<string>();
        buildingGridPositions = new List<Vector2Int_Save>();
        buildingOrientations = new List<Orientation>();
    }

    public void AddData(PlaceableInfo info)
    {
        buildingIDs.Add(info.objectData.ItemID);
        buildingGridPositions.Add(new Vector2Int_Save(info.inWorldObject.blueprint.GridPosition));
        buildingOrientations.Add(info.inWorldObject.blueprint.currentOrientation);
    }
}

// Json Vector2Int
[System.Serializable]
public struct Vector2Int_Save
{
    public int x, y;

    public Vector2Int_Save(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2Int_Save(Vector2Int original)
    {
        this.x = original.x;
        this.y = original.y;
    }
}