using UnityEngine;

public class SpriteOrder : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    private const int SortMultiplier = 30;

    public float centerOffset = 0;

    [Header("Debug")]
    [Tooltip("Draws the point (isometric ground anchor) used for depth sorting.")]
    public bool showPivotGizmo = false;

    // The transform whose Y drives depth sorting. For a building this is the
    // building root (its isometric ground anchor / front-bottom of the footprint),
    // NOT this sprite's own transform, which floats up with the artwork.
    private Transform sortAnchor;

    void Awake()
    {
        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }

        sortAnchor = ResolveSortAnchor();
    }

    void Update()
    {
        if (sr == null)
        {
            return;
        }

        float depthY = GetSortY();
        int sortOrder = Mathf.RoundToInt((depthY + centerOffset) * -1 * SortMultiplier);
        sr.sortingOrder = sortOrder;
    }

    // Isometric depth is measured from the object's ground anchor, not the
    // sprite's center/top (which sits higher the taller the art is).
    private float GetSortY()
    {
        Transform anchor = sortAnchor != null ? sortAnchor : ResolveSortAnchor();
        return anchor.position.y;
    }

    // Uses the parent building's anchor when present; otherwise falls back to
    // this object's own transform (generic, non-isometric use).
    private Transform ResolveSortAnchor()
    {
        IsometricBuilding building = GetComponentInParent<IsometricBuilding>();
        return building != null ? building.transform : transform;
    }

    private void OnDrawGizmos()
    {
        if (!showPivotGizmo)
        {
            return;
        }

        Transform anchor = sortAnchor != null ? sortAnchor : ResolveSortAnchor();
        Vector3 pivot = new Vector3(
            anchor.position.x,
            anchor.position.y + centerOffset,
            anchor.position.z);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(pivot, 0.03f);
        // Horizontal line to make the exact sort height easy to read.
        Gizmos.DrawLine(pivot - Vector3.right * 0.15f, pivot + Vector3.right * 0.15f);
    }
}
