using System;
using UnityEngine;
using UnityEngine.UI;

public class MovingIndicator : MonoBehaviour
{
    private Animator anim;
    [SerializeField] private Image fillBar;

    private bool inProgress = false;

    private Func<float> valueGetter;
    private Func<float> maxGetter;

    [SerializeField] private Canvas parentCanvas;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (inProgress && valueGetter != null && maxGetter != null)
        {
            UpdatePositionToTouch();

            float value = valueGetter();
            float max = maxGetter();
            // Update your UI visuals here
            fillBar.fillAmount = value / max;
        }
    }

    void UpdatePositionToTouch()
    {
        if (Input.touchCount > 0)
        {
            Vector2 screenPos = Input.GetTouch(0).position;
            Vector2 anchoredPos;

            // This assumes your object is a child of the Canvas
            Canvas parentCanvas = GetComponentInParent<Canvas>();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPos,
                parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera,
                out anchoredPos);

            ((RectTransform)transform).anchoredPosition = anchoredPos;
        }

#if UNITY_EDITOR
        // Optional: For mouse support in Editor
        else if (Input.GetMouseButton(0))
        {
            Vector2 screenPos = Input.mousePosition;
            Vector2 anchoredPos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPos,
                parentCanvas.worldCamera,
                out anchoredPos);

            ((RectTransform)transform).anchoredPosition = anchoredPos;
        }
#endif
    }

    /// <summary>
    /// Usage: Activate(() => startingValue, () => maximumValue) 
    /// </summary>
    /// <param name="currentGetter"></param>
    /// <param name="maximumGetter"></param>
    public void Activate(Func<float> startingValue, Func<float> maximumValue)
    {
        if (anim) anim.SetTrigger("Activate");

        valueGetter = startingValue;
        maxGetter = maximumValue;
        inProgress = true;
    }

    public void Deactivate()
    {
        if (!inProgress)
        {
            return;
        }
        if (anim) anim.SetTrigger("Deactivate");
        inProgress = false;
    }
}
