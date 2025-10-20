using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BuildMenuController : MonoBehaviour
{
    [Header("References")]
    private PlayerInventory inventory;

    [Header("Containers")]
    public List<BuildMenuSlot> slots = new();
    [SerializeField] private BuildMenuSlot buildSlot;
    [SerializeField] private Transform content;

    private void Start()
    {
        inventory = PlayerInventory.Instance;
        inventory.OnBackpackModified += (i) =>
        {
            InitializeDisplay();
        };
    }

    private void OnEnable()
    {
        InitializeDisplay();
    }

    private void InitializeDisplay()
    {
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i] != null)
                Destroy(slots[i].gameObject);
        }
        slots.Clear();

        if (inventory == null)
        {
            Debug.LogWarning($"Can not find PlayerInventory");
            return;
        }

        var buildings = inventory.GetBackpack(ItemCategory.Building);

        foreach (var b in buildings)
        {
            var slot = Instantiate(buildSlot, content);
            slot.AssignBuilding(b.Value.item, b.Value.quantity);
            slot.OnDestroy += Validate;
            slots.Add(slot);
        }
    }

    private void Validate(BuildMenuSlot slot)
    {
        // OR if you're cleaning up your own list directly:
        slots.RemoveAll(s => s == null || s.Equals(null));
    }

    
}
