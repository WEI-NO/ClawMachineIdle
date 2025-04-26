using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scenes
{
    Home,
    ClawMachine
}

public class SceneHandler : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.GetSceneByBuildIndex(0);
    }
}
