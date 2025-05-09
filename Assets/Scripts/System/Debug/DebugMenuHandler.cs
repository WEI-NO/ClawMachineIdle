using UnityEngine;

public class DebugMenuHandler : MonoBehaviour
{
    [Header("Debug Menu Properties")]
    [SerializeField] private GameObject container;
    private bool containerVisibility = false;

    [Header("Scene Cheats Properties")]
    [SerializeField] private SceneField baseScene;
    [SerializeField] private SceneField machineScene;


    private void Start()
    {
        containerVisibility = container.activeSelf;
    }


    #region Loading Cheats

    public void AddLoadingInstance()
    {
        LoadingScreen.Instance.AddLoad();
    }

    public void RemoveLoadingInstance()
    {
        LoadingScreen.Instance.RemoveLoad();
    }

    #endregion loading cheats

    #region Scene Cheats

    public void Go_BaseScene()
    {
        SceneHandler.Instance.SimulateLoadingScene(baseScene);
    }

    public void Go_MachineScene()
    {
        SceneHandler.Instance.SimulateLoadingScene(machineScene);
    }

    #endregion scene cheats

    #region Show / Hide
    public void ToggleContainer()
    {
        if (containerVisibility)
        {
            HideContainer();
        } else
        {
            ShowContainer();
        }
        containerVisibility = container.activeSelf;
    }

    private void ShowContainer()
    {
        container.SetActive(true);
    }
    private void HideContainer()
    {
        container.SetActive(false);
    }
    #endregion show / hide
}
