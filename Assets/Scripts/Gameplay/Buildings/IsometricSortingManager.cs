using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Footprint-based isometric sorting. Each registered building is represented
/// by its full ground footprint as ranges on the two isometric floor axes.
/// Definite "behind" relations become directed edges; a topological sort turns
/// the partial order into stable SortingGroup orders. Recalculates only when
/// something changes (dirty flag), once per frame in LateUpdate.
/// </summary>
public class IsometricSortingManager : MonoBehaviour
{
    private static IsometricSortingManager instance;

    // May be null (e.g. during shutdown); callers use Instance?.Method().
    public static IsometricSortingManager Instance => instance;

    // Ensures a manager exists at runtime without requiring manual scene setup.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
        {
            return;
        }

        GameObject go = new GameObject(nameof(IsometricSortingManager));
        instance = go.AddComponent<IsometricSortingManager>();
        DontDestroyOnLoad(go);
    }

    [Tooltip("Sorting order assigned to the backmost building; each building in front gets +1.")]
    [SerializeField] private int baseSortingOrder = 0;

    [Tooltip("Flip if buildings render in reversed depth order in your room orientation.")]
    [SerializeField] private bool invertDepthComparison = false;

    [SerializeField] private bool debugLogCycles = true;

    private readonly List<IsometricBuilding> buildings = new List<IsometricBuilding>();
    private bool sortingDirty;

    // Reused between rebuilds to avoid per-frame allocations.
    private readonly Dictionary<IsometricBuilding, IsometricDepthBounds> cachedBounds =
        new Dictionary<IsometricBuilding, IsometricDepthBounds>();
    private readonly Dictionary<IsometricBuilding, List<IsometricBuilding>> edges =
        new Dictionary<IsometricBuilding, List<IsometricBuilding>>();
    private readonly Dictionary<IsometricBuilding, int> incomingEdges =
        new Dictionary<IsometricBuilding, int>();
    private readonly List<IsometricBuilding> sortedResult = new List<IsometricBuilding>();
    private readonly HashSet<IsometricBuilding> processed = new HashSet<IsometricBuilding>();

    private enum IsometricDepthRelation
    {
        None,
        ABehindB,
        BBehindA
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void LateUpdate()
    {
        if (!sortingDirty)
        {
            return;
        }

        sortingDirty = false;
        RebuildSortingOrder();
    }

    #region Registration

    public void Register(IsometricBuilding building)
    {
        if (building == null || buildings.Contains(building))
        {
            return;
        }

        buildings.Add(building);
        sortingDirty = true;
    }

    public void Unregister(IsometricBuilding building)
    {
        if (buildings.Remove(building))
        {
            sortingDirty = true;
        }
    }

    public void MarkDirty()
    {
        sortingDirty = true;
    }

    #endregion

    #region Rebuild

    private void RebuildSortingOrder()
    {
        buildings.RemoveAll(b => b == null);
        if (buildings.Count == 0)
        {
            return;
        }

        // 1. Cache each building's depth bounds once for this rebuild.
        cachedBounds.Clear();
        edges.Clear();
        incomingEdges.Clear();
        for (int i = 0; i < buildings.Count; i++)
        {
            IsometricBuilding b = buildings[i];
            cachedBounds[b] = b.GetDepthBounds();
            edges[b] = new List<IsometricBuilding>();
            incomingEdges[b] = 0;
        }

        // 2. Compare every pair and add a directed edge for definite relations.
        for (int i = 0; i < buildings.Count; i++)
        {
            IsometricBuilding a = buildings[i];
            IsometricDepthBounds boundsA = cachedBounds[a];

            for (int j = i + 1; j < buildings.Count; j++)
            {
                IsometricBuilding b = buildings[j];
                IsometricDepthRelation relation = CompareDepth(boundsA, cachedBounds[b]);

                if (relation == IsometricDepthRelation.ABehindB)
                {
                    AddDependency(a, b);
                }
                else if (relation == IsometricDepthRelation.BBehindA)
                {
                    AddDependency(b, a);
                }
            }
        }

        // 3. Topologically sort into a stable back-to-front list.
        TopologicalSort(sortedResult);

        // 4. Assign increasing SortingGroup orders (behind first).
        for (int i = 0; i < sortedResult.Count; i++)
        {
            sortedResult[i].SetSortingOrder(baseSortingOrder + i);
        }
    }

    // Edge behind -> inFront means "behind" must be drawn before "inFront".
    private void AddDependency(IsometricBuilding behind, IsometricBuilding inFront)
    {
        List<IsometricBuilding> outgoing = edges[behind];
        if (outgoing.Contains(inFront))
        {
            return;
        }

        outgoing.Add(inFront);
        incomingEdges[inFront]++;
    }

    #endregion

    #region Depth Comparison

    private IsometricDepthRelation CompareDepth(IsometricDepthBounds a, IsometricDepthBounds b)
    {
        // A is entirely behind B when its whole footprint sits past B on either
        // isometric floor axis. Both directions are tested so diagonal,
        // side-by-side buildings resolve to None rather than a false relation.
        bool aBehindB =
            a.MinIsoX >= b.MaxIsoX ||
            a.MinIsoY >= b.MaxIsoY;

        bool bBehindA =
            b.MinIsoX >= a.MaxIsoX ||
            b.MinIsoY >= a.MaxIsoY;

        if (aBehindB && !bBehindA)
        {
            return invertDepthComparison ? IsometricDepthRelation.BBehindA : IsometricDepthRelation.ABehindB;
        }

        if (bBehindA && !aBehindB)
        {
            return invertDepthComparison ? IsometricDepthRelation.ABehindB : IsometricDepthRelation.BBehindA;
        }

        return IsometricDepthRelation.None;
    }

    #endregion

    #region Topological Sort

    private void TopologicalSort(List<IsometricBuilding> result)
    {
        result.Clear();
        processed.Clear();

        // Stable base order so unrelated (ready-at-once) buildings never flicker.
        buildings.Sort(CompareFallback);

        int count = buildings.Count;
        while (result.Count < count)
        {
            IsometricBuilding next = null;

            // Pick the first unprocessed building with no remaining incoming
            // edges, in deterministic fallback order.
            for (int i = 0; i < buildings.Count; i++)
            {
                IsometricBuilding candidate = buildings[i];
                if (!processed.Contains(candidate) && incomingEdges[candidate] == 0)
                {
                    next = candidate;
                    break;
                }
            }

            if (next == null)
            {
                // Remaining buildings form a cycle: append them in fallback order.
                HandleCycle(result);
                return;
            }

            processed.Add(next);
            result.Add(next);

            List<IsometricBuilding> outgoing = edges[next];
            for (int i = 0; i < outgoing.Count; i++)
            {
                incomingEdges[outgoing[i]]--;
            }
        }
    }

    private void HandleCycle(List<IsometricBuilding> result)
    {
        if (debugLogCycles)
        {
            Debug.LogWarning(
                "Isometric sorting cycle detected. Falling back to stable grid order for unresolved buildings.");
        }

        // buildings is already sorted by CompareFallback.
        for (int i = 0; i < buildings.Count; i++)
        {
            IsometricBuilding building = buildings[i];
            if (!processed.Contains(building))
            {
                processed.Add(building);
                result.Add(building);
            }
        }
    }

    // Backmost first: higher grid Y, then lower grid X, then instance id.
    private static int CompareFallback(IsometricBuilding a, IsometricBuilding b)
    {
        int yComparison = b.blueprint.GridPosition.y.CompareTo(a.blueprint.GridPosition.y);
        if (yComparison != 0)
        {
            return yComparison;
        }

        int xComparison = a.blueprint.GridPosition.x.CompareTo(b.blueprint.GridPosition.x);
        if (xComparison != 0)
        {
            return xComparison;
        }

        return a.GetInstanceID().CompareTo(b.GetInstanceID());
    }

    #endregion
}
