using CustomLibrary.References;
using UnityEngine;

public class MainDatabase : MonoBehaviour
{
    public static MainDatabase Instance;

    [Header("Database Properties")]
    // Furnitures
    [SerializeField] private PlaceableDatabase placeableDatabase;
    // Prize
    [SerializeField] private PrizeDatabase prizeDatabase;
    [SerializeField] private  CurrencyDatabase currencyDatabase;

    public PlaceableDatabase DB_Placeable { get { return placeableDatabase; } private set { } }
    public PrizeDatabase DB_Prize { get { return prizeDatabase; } private set { } }
    public CurrencyDatabase DB_Currency { get { return currencyDatabase; } private set { } }

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

}
