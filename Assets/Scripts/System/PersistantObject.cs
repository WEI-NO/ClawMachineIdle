using CustomLibrary.References;
using UnityEngine;

public class PersistantObject : MonoBehaviour
{
    public static PersistantObject Instance;

    [Header("References")]
    public SceneHandler _SceneHandler;

    void Awake()
    {
        Initializer.SetInstance(this);
        DontDestroyOnLoad(this.gameObject);

        // Get Components/References
        _SceneHandler = GetComponent<SceneHandler>();

    }
}
