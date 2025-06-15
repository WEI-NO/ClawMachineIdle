using CustomLibrary.References;
using UnityEngine;

public class PersistentCanvas : MonoBehaviour
{
    public static PersistentCanvas Instance;

    public ItemCountDisplay coinDisplay;
    public ItemCountDisplay ticketDisplay;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }
}
