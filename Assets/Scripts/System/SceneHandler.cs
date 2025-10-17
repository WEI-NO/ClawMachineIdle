using CustomLibrary.References;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scenes
{
    Home,
    ClawMachine
}


public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance;


    [Header("Scene Properties")]
    //[SerializeField] private SceneField _persistentGameplay;
    [SerializeField] private SceneField _sceneToLoad;
    private List<AsyncOperation> _scenesLoading = new List<AsyncOperation>();
    private Coroutine _sceneSwitchProcess = null;

    public Action OnSceneLoaded;

    private void Awake()
    {
        Initializer.SetInstance(this);

        string persistentSceneName = "Persistent Gameplay";
        string defaultSceneName = "Room Sandbox";

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene.name != persistentSceneName)
            {
                _sceneToLoad = new SceneField(loadedScene.name);
                Debug.Log($"Detected initial gameplay scene: {_sceneToLoad.SceneName}");
                return;
            }
        }

        // Default to Room Sandbox
        _sceneToLoad = new SceneField(defaultSceneName);
        Debug.LogWarning($"No gameplay scene found. Defaulting to: {defaultSceneName}");
        LoadScene();
    }

    public SceneField CurrentScene()
    {
        return _sceneToLoad;
    }

    private void Start()
    {

    }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsurePersistentSceneLoaded()
    {
        string persistentName = "Persistent Gameplay";

        // Already in build settings but loading — check if it's part of loaded scenes
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == persistentName)
                return;
        }

        // Not loaded or loading yet — load it additively
        SceneManager.LoadSceneAsync(persistentName, LoadSceneMode.Additive);
    }

    /// <summary>
    /// Loads the persistent gameplay scene (if not loaded), then loads the current _sceneToLoad.
    /// </summary>
    private void LoadScene()
    {
        _scenesLoading.Clear();

        //// Ensure the persistent scene is loaded additively if not already loaded
        //if (!SceneManager.GetSceneByName(_persistentGameplay).isLoaded)
        //{
        //    _scenesLoading.Add(SceneManager.LoadSceneAsync(_persistentGameplay, LoadSceneMode.Additive));
        //}

        // Load_Room the target scene additively
        if (!SceneManager.GetSceneByName(_sceneToLoad).isLoaded)
        {
            _scenesLoading.Add(SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Additive));
        }
    }

    /// <summary>
    /// Changes the current scene to a new one. Optionally loads the new scene and removes the old one.
    /// </summary>
    /// <param name="scene">New scene to load.</param>
    /// <param name="switchScene">Whether to load the new scene immediately.</param>
    /// <param name="remove">Whether to unload the previous _sceneToLoad scene.</param>
    public void ChangeScene(SceneField scene, bool switchScene = true, bool remove = true)
    {
        if (remove)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene sceneInList = SceneManager.GetSceneAt(i);
                if (sceneInList.name == _sceneToLoad)
                    SceneManager.UnloadSceneAsync(_sceneToLoad);
            }
        }

        _sceneToLoad = scene;

        if (switchScene)
        {
            LoadScene();
        }
    }

    public void SimulateLoadingScene(SceneField scene, float initialDelay = 0.5f, float loadingDelay = 2.0f)
    {
        if (_sceneSwitchProcess != null)
        {
            return;
        }

        _sceneSwitchProcess = StartCoroutine(SceneSimulator(scene, initialDelay, loadingDelay));
    }

    private IEnumerator SceneSimulator(SceneField scene, float initialDelay, float loadingDelay)
    {
        LoadingScreen.Instance.AddLoad();
        yield return new WaitForSeconds(initialDelay);
        ChangeScene(scene);
        yield return new WaitForSeconds(loadingDelay);

        LoadingScreen.Instance.RemoveLoad();
        _sceneSwitchProcess = null;
        OnSceneLoaded?.Invoke();
        OnSceneLoaded = null;
    }

    public void ListenForSceneChange(Action action)
    {
        OnSceneLoaded += action;
    }


}
