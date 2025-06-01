using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MainHUD_UI : MonoBehaviour
{
    SceneHandler sceneHandler;

    [SerializeField] private int sceneIndex;
    [SerializeField] private List<SceneField> alternatingScenes;

    public bool inTransition = false;

    void Start()
    {
        sceneHandler = SceneHandler.Instance;
        if (sceneHandler)
        {
            var scene = SceneHandler.Instance.CurrentScene();
            for (int i = 0; i < alternatingScenes.Count; i++)
            {
                if (scene.Compare(alternatingScenes[i]))
                {
                    sceneIndex = i;
                }
            }
        }
    }

    #region Home Button

    public void AlternateScene()
    {
        if (inTransition) return;
        sceneIndex = (sceneIndex + 1) % alternatingScenes.Count;

        GoToScene(alternatingScenes[sceneIndex]);
        inTransition = true;
        SceneHandler.Instance.ListenForSceneChange(() => { inTransition = false; });
    }

    private void GoToScene(SceneField scene)
    {
        sceneHandler.SimulateLoadingScene(scene);
    }

    #endregion home button
}
