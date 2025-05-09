using UnityEngine;

public enum ItemCategory
{
    Currency,
    Building,
    Backpack
}

public class BaseItem : MonoBehaviour
{
    #region Base Class
    protected virtual void OnAwake() { }
    protected virtual void OnEnabled() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnDisabled() { }
    protected virtual void OnDestroyed() { }

    private void Awake()
    {
        OnAwake();
    }
    private void OnEnable()
    {
        OnEnabled();
    }
    private void Start()
    {
        OnStart();
    }
    private void Update()
    {
        OnUpdate();
    }
    private void FixedUpdate()
    {
        OnFixedUpdate();
    }
    private void OnDisable()
    {
        OnDisabled();
    }
    private void OnDestroy()
    {
        OnDestroyed();
    }
    #endregion base class

    [Header("Item Properties (Private)")]
    [SerializeField] private string _itemName;
    [SerializeField] private Sprite _itemIcon;
    [SerializeField] private ItemCategory _itemType;

    [Header("Public Properties")]
    public string ItemName { get { return _itemName; } private set { } }
    public Sprite ItemIcon { get { return _itemIcon; } private set { } }
    public ItemCategory ItemType { get { return _itemType; } private set { } }


}
