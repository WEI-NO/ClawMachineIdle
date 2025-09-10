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
    [Header("Effect Settings")]
    [SerializeField] private GameObject effect;
    [SerializeField] private bool onUI = true;
    
    
    protected void SetQuantity(int q)
    {
        Quantity = q;
    }

    public void PrizeClaim()
    {
        DefaultPrizeClaim();
        Destroy(gameObject);
    }

    public void DefaultPrizeClaim()
    {
        PrizeClaimFunction();
        PrizeClaimEffectSpawn();
    }

    public virtual void PrizeClaimEffectSpawn()
    {
        // Spawn Effect
        if (effect)
        {
            if (onUI && PersistentCanvas.Instance != null)
            {
                // Convert world position to screen space
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

                // Instantiate effect under MainCanvas
                GameObject uiEffect = Instantiate(effect, screenPos, Quaternion.identity, PersistentCanvas.Instance.transform);
            }
            else
            {
                // Spawn effect in world space
                Instantiate(effect, transform.position, Quaternion.identity);
            }
        }
    }

    public virtual void PrizeClaimFunction()
    {
        if (RewardItem)
        {
            PlayerInventory.Instance.GiveItem(RewardItem, Quantity);
        }
    }

    

}
