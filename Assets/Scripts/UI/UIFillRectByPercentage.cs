using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to a UI element (Image, RawImage, etc.) on a Canvas.
/// Stretches the UI element from the bottom by a percentage of the screen/canvas height.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIFillRectByPercentage : MonoBehaviour
{
    [Range(0f, 1f)]
    public float fillPercent = 0.5f; // 0 = invisible, 1 = fill entire canvas height

    private RectTransform rectTransform;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("UIFillRectByPercentage: No Canvas found in parent hierarchy.");
        }
    }

    void Start()
    {
        UpdateRect();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        UpdateRect();
    }
#endif

    /// <summary>
    /// Updates the UI element to fill from the bottom by the desired percentage.
    /// </summary>
    public void UpdateRect()
    {
        if (rectTransform == null)
            return;

        // Anchor to bottom stretch (left=0, right=1, bottom=0, top=fillPercent)
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(1f, fillPercent);

        // Set offsets to zero to fully stretch within the anchors
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    // If you want to adjust at runtime
    void Update()
    {
        // Optionally, only update if fillPercent has changed (add logic if you want)
        UpdateRect();
    }
}