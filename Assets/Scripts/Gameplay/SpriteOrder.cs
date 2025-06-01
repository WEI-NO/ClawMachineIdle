using UnityEngine;

public class SpriteOrder : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    private const int SortMultiplier = 30;
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        int sortOrder = Mathf.RoundToInt(transform.position.y * -1 * SortMultiplier);
        sr.sortingOrder = sortOrder;
    }
}
