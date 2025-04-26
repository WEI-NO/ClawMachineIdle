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

    [Header("State Properties")]
    public bool SnapToGrid = true;
    public GridObjectState objectState = GridObjectState.Invalid;

    [Header("Grid Properties")]
    public GridType occupiedType;

    /// <summary>
    /// Called in Update() and snaps the building to Grid if SnapToGrid flag is true.
    /// </summary>
    private void SnapToGridUpdate()
    {
        if (!SnapToGrid || Grid2D.Instance == null) return;
        if (GridControlledObject == null) return;

        float offsetAxis = Grid2D.Instance.cellSize / 2.0f;
        Vector2 offset = new Vector2(offsetAxis, offsetAxis); 

        Vector2Int cellPos = Grid2D.Instance.GetGridPosition((Vector2)transform.position + offset);

        GridPosition = cellPos;
        GridControlledObject.position = new Vector2(GridPosition.x, GridPosition.y) * Grid2D.Instance.cellSize;
    }

    #region State

    private void StateMachineUpdate()
    {

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
    
    public void OnMovingState_Start()
    {

    }

    public void OnMovingState_Update()
    {

    }

    public void OnInWorldState_Start()
    {

    }

    public void OnInWorldState_Update()
    {

    }

    #endregion state
}
