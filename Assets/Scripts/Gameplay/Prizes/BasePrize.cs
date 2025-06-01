using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class BasePrize : MonoBehaviour
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
        RewardItem = MainDatabase.Instance.DB_Currency.GetDataByID(Reward);
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

    [Header("Prize Settings")]
    public string Reward;
    public int Quantity = 1;
    public BaseItem RewardItem;
    public ItemRarity ItemRarity;
    public bool isEgg;
    
    protected void SetQuantity(int q)
    {
        Quantity = q;
    }

    public void PrizeClaim()
    {
        PrizeClaimFunction();
        Destroy(gameObject);
    }

    public virtual void PrizeClaimFunction()
    {
        if (RewardItem)
        {
            PlayerInventory.Instance.GiveItem(RewardItem, Quantity);
        }
    }

    

}
