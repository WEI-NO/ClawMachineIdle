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
    private static string FullPath(string filename)
    {
        return Path.Combine(Application.persistentDataPath, filename);
    }

    #region Public Interface - Room Saving
    // Room Data
    private static RoomData roomData;
    private const string RoomDataFileName = "roomdata.json";

    public static bool SaveRoom(List<PlaceableInfo> infos)
    {
        Debug.Log("Saving Room");

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
        Vector2 j = new();
        return false;
    }

       
    public static RoomData Load_Room()
    {
        Debug.Log("Loading Room");

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

    #region Public Interface - Inventory Saving
    // Room Data
    private static InventoryData inventoryData;
    private const string InventoryDataFileName = "inventorydata.json";

    public static bool SaveInventory(List<InventoryItem> infos)
    {
        Debug.Log("Saving Inventory");
        inventoryData = new InventoryData();
        foreach (var i in infos)
        {
            inventoryData.AddData(i);
        }

        string path = FullPath(InventoryDataFileName);

        try
        {
            var json = JsonUtility.ToJson(inventoryData);

            // To prevent crashes mid saving
            var tempPath = path + ".tmp";
            File.WriteAllText(tempPath, json);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tempPath, path);

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Save Syste: {e}");
        }

        return false;
    }


    public static InventoryData Load_Inventory()
    {
        Debug.Log("Loading Inventory");

        // Creates a new save if nothing is found
        string path = FullPath(InventoryDataFileName);

        try
        {
            if (!File.Exists(path))
                return new InventoryData();

            var json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<InventoryData>(json);
            return data ?? new InventoryData();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Load failed, creating new data. Reason: {e}");
            return new InventoryData();
        }

    }

    #endregion public interface - room saving

}
public abstract class DataSet<TData> where TData : class
{
    public abstract void AddData(TData data);

    public abstract bool Validate();
}


#region Room Data
// Room Data
public class RoomData : DataSet<PlaceableInfo>
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

    public override void AddData(PlaceableInfo info)
    {
        buildingIDs.Add(info.objectData.ItemID);
        buildingGridPositions.Add(new Vector2Int_Save(info.inWorldObject.blueprint.GridPosition));
        buildingOrientations.Add(info.inWorldObject.blueprint.currentOrientation);
    }

    public override bool Validate()
    {
        if (buildingIDs.Count != buildingGridPositions.Count) return false;
        if (buildingOrientations.Count != buildingGridPositions.Count) return false;

        return buildingIDs != null && buildingGridPositions != null && buildingOrientations != null;
    }
}
#endregion room data

#region Inventory Data
public class InventoryData : DataSet<InventoryItem>
{
    public List<string> itemIDs = new List<string>();
    public List<int> itemQuantities = new List<int>();

    public InventoryData()
    {
        itemIDs = new List<string>();
        itemQuantities = new List<int>();
    }

    public override void AddData(InventoryItem data)
    {
        itemIDs.Add(data.ItemID);
        itemQuantities.Add(data.quantity);
    }

    public override bool Validate()
    {
        if (itemIDs.Count != itemQuantities.Count) return false;

        return itemIDs != null && itemQuantities != null;
    }
}
#endregion Inventory Data

#region Json Serializable Structs
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
#endregion Json Serializable Structs
