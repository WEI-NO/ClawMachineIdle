using CustomLibrary.References;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MainHUD_UI : MonoBehaviour
{
    public static MainHUD_UI Instance;
    SceneHandler sceneHandler;

    [SerializeField] private int sceneIndex;
    [SerializeField] private List<SceneField> alternatingScenes;

    public bool inTransition = false;

    [Header("Animation Properties")]
    [SerializeField] private Vector2 yRange = new Vector2(0, 1f);
    [SerializeField] private float animationSpeed = 1.0f;
    private Coroutine movingCoroutine = null;
    private bool currentState = true;
    

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

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

            sceneHandler.OnSceneLoaded += () => ToggleState(true);
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

    #region Animation

    public void ToggleState(bool state)
    {
        //if (state == currentState)
        //{
        //    return;
        //}
        currentState = state;
        if (movingCoroutine != null)
        {
            StopCoroutine(movingCoroutine);
        }
        movingCoroutine = StartCoroutine(SetTargetYScale(state ? yRange.y : yRange.x, animationSpeed));
    }


    private IEnumerator SetTargetYScale(float y, float speed)
    {
        float currY = transform.localScale.y;
        float dist = Mathf.Abs(y - currY);
        while (dist > 0.001f)
        {
            float lerpedY = Mathf.Lerp(currY, y, Time.deltaTime * speed);
            transform.localScale = new Vector3(transform.localScale.x, lerpedY, transform.localScale.z);
            currY = transform.localScale.y;
            dist = Mathf.Abs(y - currY);
            yield return null;
        }
    }

    #endregion animation
}
