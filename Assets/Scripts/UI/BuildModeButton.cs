using UnityEngine;

public class BuildModeButton : MonoBehaviour
{
    [Header("References")]
    SceneHandler sceneHandler;
    [SerializeField] private GameObject button;


    private void Start()
    {
        sceneHandler = SceneHandler.Instance;
        sceneHandler.OnSceneChanged += OnSceneLoaded;
        OnSceneLoaded(sceneHandler.CurrentScene());
    }

    void OnSceneLoaded(SceneField s)
    {
        if (s.SceneName == "Room")
        {
            button.SetActive(true);
        } else if (s.SceneName == "Claw")
        {
            button.SetActive(false);
        }
    }

    public void EnableBuildMode()
    {
        if (TouchController.Instance != null)
        {
            TouchController.Instance.SetEditMode(true);
        }
    }
}
