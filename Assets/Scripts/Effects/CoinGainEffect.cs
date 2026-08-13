using CustomLibrary.Math.Vector;
using System.Collections;
using UnityEngine;

public class CoinGainEffect : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private int _coinAmount;

    [Header("Main Sequence Settings")]
    [SerializeField] private Vector2 radialOffsetRange = new Vector2(-50, 50);
    [SerializeField] private Vector2 coinDelayRange;

    [Header("Scale Sequence Settings")]
    [SerializeField] private float scaleSpeed;
    [SerializeField] private float scaleTarget;

    [Header("Move Sequence Settings")]
    [SerializeField] private Vector2 moveDelayRange;
    [SerializeField] private float moveSpeed;
    [SerializeField] private RectTransform targetElement;


    [SerializeField] private GameObject original;

    private Coroutine coinScaleSequence = null;

    private void Start()
    {
        targetElement = PersistentCanvas.Instance.coinDisplay.transform as RectTransform;
        StartCoroutine(CoinGainSequnece());
    }

    private void Update()
    {
        if (transform.childCount <= 0) Destroy(gameObject);
    }
    IEnumerator CoinGainSequnece()
    {
        if (!original) yield break;     

        for (int i = 0; i < _coinAmount; i++)
        {
            var coin = Instantiate(original, transform, false).transform as RectTransform;
            float xOffset = Random.Range(radialOffsetRange.x, radialOffsetRange.y);
            float yOffset = Random.Range(radialOffsetRange.x, radialOffsetRange.y);
            coin.anchoredPosition += new Vector2(xOffset, yOffset);
            coinScaleSequence = StartCoroutine(CoinScaleSequence(coin, 0, scaleTarget));
            StartCoroutine(CoinMoveSequence(coin, moveSpeed));
        }
        yield return null;
    }

    IEnumerator CoinScaleSequence(RectTransform transform, float startScale, float endScale)
    {
        if (!transform) yield break;

        transform.localScale = new Vector2(startScale, startScale);
        float current = transform.localScale.x;
        while (Mathf.Abs(current - endScale) > 0.001f)
        {
            if (!transform) yield break;
            float newScale = Mathf.Lerp(current, endScale, Time.deltaTime * scaleSpeed);
            transform.localScale = new Vector2(newScale, newScale);
            current = transform.localScale.x;
            yield return null;
        }

        if (transform != null)
            transform.localScale = new Vector2(endScale, endScale);
    }

    IEnumerator CoinMoveSequence(RectTransform transform, float moveSpeed = 5f)
    {
        if (!transform || !targetElement) yield break;

        

        yield return new WaitForSeconds(moveDelayRange.GetRandom());

        // Convert target element's world position to screen space
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, targetElement.GetChild(2).position);

        // Convert screen point to local space in the transform's parent canvas
        RectTransform parentRect = transform.parent as RectTransform;
        Vector2 localTarget;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, null, out localTarget);

        Vector2 startPos = transform.anchoredPosition;

        // Control point with randomized arc
        Vector2 midPoint = (startPos + localTarget) / 2f;
        float arcHeight = Random.Range(50f, 150f);
        midPoint.y += arcHeight;
        midPoint.x += Random.Range(-50f, 50f);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;

            Vector2 curvedPos =
                Mathf.Pow(1 - t, 2) * startPos +
                2 * (1 - t) * t * midPoint +
                Mathf.Pow(t, 2) * localTarget;

            transform.anchoredPosition = curvedPos;
            yield return null;
        }

        transform.anchoredPosition = localTarget;
        StartCoroutine(DespawnSequence(transform));
    }

    public IEnumerator DespawnSequence(RectTransform target)
    {
        PersistentCanvas.Instance.coinDisplay.TriggerJump();
        yield return null;
        Destroy(target.gameObject);
        if (coinScaleSequence != null)
        {
            StopCoroutine(coinScaleSequence);
        }
    }


}
