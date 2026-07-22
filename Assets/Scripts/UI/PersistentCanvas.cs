using CustomLibrary.References;
using UnityEngine;
using System.Collections.Generic;

public class PersistentCanvas : MonoBehaviour
{
    public static PersistentCanvas Instance;

    private int id_counter = 0;

    public ItemCountDisplay coinDisplay;
    public ItemCountDisplay ticketDisplay;

    public List<OpenableUI> openableUIs = new List<OpenableUI>();
    public bool autoClose = true;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        FindOpenableUI();
    }

    void FindOpenableUI()
    {
        foreach (Transform child in transform)
        {
            var openable = child.GetComponent<OpenableUI>();
            if (openable == null)
                continue;

            openableUIs.Add(openable);
            openable.SetOpenableUI_id(id_counter++);
        }
    }

    public void TriggerOpen(OpenableUI ui)
    {
        foreach (var o in openableUIs)
        {
            if (ui.GetOpenableUI_id() == o.GetOpenableUI_id())
                continue;
            o.Toggle_Off();
        }
    }


}
