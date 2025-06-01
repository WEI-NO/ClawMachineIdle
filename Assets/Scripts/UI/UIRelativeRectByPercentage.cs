using UnityEngine;

/// <summary>
/// Scales a UI element's height to a percent of the screen height,
/// but reduces the height as the aspect ratio increases (wider screens = shorter button).
/// Keeps aspect ratio of the sprite.
/// Updates automatically whenever screen resolution changes.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIAspectRelativeButton : MonoBehaviour
{
    [Tooltip("Button height will be sizePercent * screen height * (height/width)")]
    public float sizePercent = 0.2f;      // 0.2 = 20% of screen height before aspect scaling
    public float spritePixelWidth = 100f; // Your sprite's original width in pixels
    public float spritePixelHeight = 100f;// Your sprite's original height in pixels

    private RectTransform rectTransform;
    private int lastScreenWidth = 0;
    private int lastScreenHeight = 0;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        UpdateRect();
        CacheScreenSize();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        UpdateRect();
        CacheScreenSize();
    }
#endif

    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            UpdateRect();
            CacheScreenSize();
        }
    }

    void CacheScreenSize()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    public void UpdateRect()
    {
        if (rectTransform == null || spritePixelWidth <= 0 || spritePixelHeight <= 0)
            return;

        float aspectModifier = (float)Screen.height / (float)Screen.width;
        float targetHeight = Screen.height * sizePercent * aspectModifier;
        float aspect = spritePixelWidth / spritePixelHeight;
        float targetWidth = targetHeight * aspect;

        rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
    }
}
