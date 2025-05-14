using CustomLibrary.References;
using System;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    [Header("Loading Settings")]
    [SerializeField] private Animator anim;
    [SerializeField] private LoadingIcon _loadingIcon;
    private LoadingIcon _currentIcon;

    [Header("Loading Properties")]
    [SerializeField] private string _beginKeyword = "Begin";
    [SerializeField] private string _endKeyword = "End";
    [SerializeField] private int _loadingInstances = 0;
    private bool _inLoading = false;

    [Header("Events")]
    private Action<int> OnLoadingInstancesChange;

    private void Awake()
    {
        Initializer.SetInstance(this);

        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            AddLoad();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            RemoveLoad();
        }

        LoadingScreenStateMachineUpdate();
    }

    public void AddLoad()
    {
        _loadingInstances++;
        OnLoadingInstancesChange?.Invoke(_loadingInstances);
    }

    public void RemoveLoad()
    {
        int prevCount = _loadingInstances;
        _loadingInstances = Mathf.Clamp(--_loadingInstances, 0, _loadingInstances);
        
        if (prevCount != _loadingInstances)
        {
            OnLoadingInstancesChange?.Invoke(_loadingInstances);
        }
    }

    private void BeginLoadingScreen()
    {
        if (!anim) return;

        anim.SetTrigger(_beginKeyword);
    }

    private void EndLoadingScreen()
    {
        if (!anim) return;

        anim.SetTrigger(_endKeyword);
    }


    #region Animation

    public void SpawnLoadingIcon()
    {
        if (!_loadingIcon) return;

        if (_currentIcon)
        {
            DestroyLoadingIcon();
        }

        _currentIcon = Instantiate(_loadingIcon, transform);
    }

    public void DestroyLoadingIcon()
    {
        if (_currentIcon)
        {
            _currentIcon.TriggerEnd();
        }
    }

    #endregion animation

    #region State Machine

    private void LoadingScreenStateMachineUpdate()
    {
        if (_inLoading)
        {
            // Is in loading screen
            if (_loadingInstances == 0)
            {
                EndLoadingScreen();
                _inLoading = false;
            }
        } else
        {
            // Outside of loading screen
            if (_loadingInstances > 0)
            {
                BeginLoadingScreen();
                _inLoading = true;
            }
        }
    }

    #endregion state machine

}
