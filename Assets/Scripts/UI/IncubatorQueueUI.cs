using CustomLibrary.References;
using UnityEngine;

public class IncubatorQueueUI : MonoBehaviour
{
    public static IncubatorQueueUI Instance;

    [Header("References")]
    private IncubationController controller;
    public QueueSlotUI[] queueSlots = new QueueSlotUI[5];


    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        controller = IncubationController.Instance;

        // WIP : Use save files for initial unlock
        Initialize();
    }

    private void Initialize()
    {
        for (int i = 0; i < queueSlots.Length; i++)
        {
            // Lock when i is bigger than maxQueued
            bool unlock = i < controller.MaxQueued;
            queueSlots[i].SetLockedState(!unlock);
        }
    }

    private void Update()
    {
        for (int i = 0; i < queueSlots.Length; i++)
        {
            int eggIndex = i + 1;
            // Lock when i is bigger than maxQueued
            bool unlock = eggIndex <= controller.MaxQueued;

            if (unlock)
            {
                var egg = controller.GetEggFromQueue(eggIndex);
                if (egg == null) 
                {
                    queueSlots[i].SetContainer(null);
                    break;
                }
                queueSlots[i].SetContainer(egg);
            }

        }
    }
}
