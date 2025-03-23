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
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
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
        FollowTargetUpdate();
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

    [Header("Components")]
    public Rigidbody2D rb;
    public Collider2D col;

    [Header("Control")]
    public Transform followTarget;
    public float followStrength = 10.0f;

    #region Controls

    private void FollowTargetUpdate()
    {
        if (followTarget)
        {
            transform.position = Vector3.Lerp(transform.position, followTarget.transform.position, Time.fixedDeltaTime * followStrength);
        }
    }

    public void ActivateGrabbedState()
    {
        if (rb)
        {
            rb.gravityScale = 0.0f;
        }

        //if (col)
        //{
        //    col.isTrigger = true;
        //}
    }

    public void SetTarget(Transform target)
    {
        followTarget = target;
    }

    #endregion controls

}
