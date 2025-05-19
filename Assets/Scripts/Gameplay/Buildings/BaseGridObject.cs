using UnityEngine;

public enum GridObjectState
{
    Invalid,
    Moving, // It is currently in ghost mode
    InWorld, // Placed in world and can be interacted with
}

public class BaseGridObject : MonoBehaviour
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
        SetInWorld = false;
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
        SnapToGridUpdate();
        StateMachineUpdate();
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

    [Header("Object Properties")]
    public Vector2Int _ObjectDimension // Ensures Object Dimension is never 0.
    { 
        get
        {
            ObjectDimension.x = Mathf.Clamp(ObjectDimension.x, 1, ObjectDimension.x);
            ObjectDimension.y = Mathf.Clamp(ObjectDimension.y, 1, ObjectDimension.y);
            return ObjectDimension;
        }
        private set { }
    }
    public Vector2Int ObjectDimension = new Vector2Int(1, 1);
    public Vector2Int GridPosition;
    public Transform GridControlledObject; // The child object that represents the building.
    public SpriteRenderer ghostView;

    [Header("State Properties")]
    public bool SnapToGrid = true;
    public bool SetInWorld = false;
    public GridObjectState objectState = GridObjectState.Invalid;

    [Header("Grid Properties")]
    public GridType occupiedType;

    /// <summary>
    /// Called in Update() and snaps the building to Grid if SnapToGrid flag is true.
    /// </summary>
    private void SnapToGridUpdate()
    {
        if (!SnapToGrid || Grid2D.Instance == null || GridControlledObject == null)
            return;

        //// Offset if needed to shift where mouse detection happens
        //Vector2 worldPos = transform.position;

        //// Snap the child to the closest grid cell
        //Vector2Int cellPos = Grid2D.Instance.GetGridPosition(worldPos);
        //GridPosition = cellPos;

        // Get world position from cellPos and move the child there
        Vector2 snappedPos = Grid2D.Instance.GetWorldPosition(GridPosition);
        GridControlledObject.position = snappedPos;
    }

    #region State

    private void StateMachineUpdate()
    {
        switch (objectState)
        {
            case GridObjectState.Invalid:
                break;
            case GridObjectState.Moving:
                OnMovingState_Update();
                break;
            case GridObjectState.InWorld:
                OnInWorldState_Update();
                break;
        }
    }

    public void ChangeState(GridObjectState state)
    {
        if (state == objectState)
        {
            return;
        }

        objectState = state;
        switch(state)
        {
            case GridObjectState.Moving:
                OnMovingState_Start();
                break;
            case GridObjectState.InWorld:
                OnInWorldState_Start();
                break;
        }
    }
    
    public virtual void OnMovingState_Start()
    {
        if (ghostView) ghostView.enabled = true;
    }

    public virtual void OnMovingState_Update()
    {

    }

    public virtual void OnInWorldState_Start()
    {
        if (ghostView) ghostView.enabled = false;

        SetInWorld = true;
    }

    public virtual void OnInWorldState_Update()
    {

    }

    #endregion state
}
