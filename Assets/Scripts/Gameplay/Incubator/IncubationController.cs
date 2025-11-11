using CustomLibrary.References;
using System.Collections.Generic;
using UnityEngine;

public class IncubationController : MonoBehaviour
{
    public static IncubationController Instance;

    [Header("Incubation Properties")]
    public List<EggContainer> IncubationQueue = new List<EggContainer>();
    public int MaxQueued = 2;
    public int CurrentInQueue = 0;

    private void Awake()
    {
        //if (IncubationTimes.Length != ItemRarity.Count.ToInt())
        //{
        //    Debug.LogWarning($"{gameObject.name}: Did not implement enough incubation Times.");
        //}
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        //PlayerInventory.Instance.OnBackpackModified += NewEggAdded;
        CurrentInQueue = -1;
    }

    private void Update()
    {
        if (HasQueue() && CurrentInQueue >= 0)
        {
            EggContainer first = IncubationQueue[CurrentInQueue];
            if (first.TickTimer())
            {
                print("First egg is hatched. Waiting to claim.");
                SelectFirstInQueue();
                //RemoveFirst();
            }
        }
    }


    private void NewEggAdded(InventoryItem item)
    {
        if (item.item is EggItem)
        {
            AddToQueue(item.item);
            SelectFirstInQueue();
        }
        
    }
    

    #region Queue

    public int GetFirstInQueue_Index(bool excludeReady = true)
    {
        if (!HasQueue()) return -1;
        int index = -1;
        // Do not exclude ready eggs
        if (!excludeReady)
        {
            index = 0;
        }
        // Exclude ready eggs and just returnb the current in queue.
        else
        {
            index = CurrentInQueue;
        }
        if (index < 0 || index >= IncubationQueue.Count) return -1;

        return index;
    }

    public EggContainer GetFirstInQueue(bool excludeReady = true)
    {
        int index = GetFirstInQueue_Index(excludeReady);
        if (index < 0 || index >= IncubationQueue.Count) return null;

        return IncubationQueue[index];
    }

    public EggContainer GetEggFromQueue(int index)
    {
        if (index < 0 || index >= IncubationQueue.Count) return null;

        return IncubationQueue[index];
    }

    public bool HasQueue()
    {
        return IncubationQueue.Count > 0;
    }

    public int CountInQueue()
    {
        return IncubationQueue.Count;
    }
    
    public bool MaxQueueSpace()
    {
        return IncubationQueue.Count - 1 >= MaxQueued;
    }

    public void AddToQueue(BaseItem egg)
    {
        if (MaxQueueSpace()) return;
        if (egg is EggItem eggItem)
        {
            EggContainer container = new EggContainer(egg, eggItem.hatchTime_s);
            IncubationQueue.Add(container);
            SelectFirstInQueue();
        } else
        {
            Debug.LogWarning($"{gameObject.name}: Passed in a non EggItem item");
        }
    }

    public void RemoveAt(int index)
    {
        if (index < IncubationQueue.Count && index >= 0)
        {
            IncubationQueue.RemoveAt(index);
            
            SelectFirstInQueue();
        }
    }
    
    public void SelectFirstInQueue()
    {
        bool found = false;
        for (int i = 0; i < IncubationQueue.Count; i++)
        {
            if (i > MaxQueued)
            {
                break;
            }

            if (!IncubationQueue[i].Done()) 
            {
                found = true;
                CurrentInQueue = i;
                break;
            }

        }
        if (!found) CurrentInQueue = -1;
    }


    #endregion queue

}


[System.Serializable]
public class EggContainer
{
    public EggItem heldEgg;
    public float hatchTime;
    public float currentHatchTime;

    public bool TickTimer()
    {
        currentHatchTime += Time.deltaTime;
        return currentHatchTime >= hatchTime;
    }

    public EggContainer(BaseItem egg, float hatchTime)
    {
        if (egg is EggItem)
        {
            heldEgg = egg as EggItem;
        }
        currentHatchTime = 0;
        this.hatchTime = hatchTime;
    }

    public bool Done()
    {
        return currentHatchTime >= hatchTime;
    }
}