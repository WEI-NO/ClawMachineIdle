using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CollisionIgnorer : MonoBehaviour
{
    [Header("Collision Ignore Properties")]
    public List<IgnoredLayers> IgnoreLayers;

    private void Start()
    {
        ApplyIgnoreLayers();   
    }

    #region Layer Ignores

    /// <summary>
    /// Goes through the IgnoreLayers list and apply all ignore layers.
    /// </summary>
    private void ApplyIgnoreLayers()
    {
        foreach (var layers in IgnoreLayers)
        {
            layers.Apply();
        }
    }

    #endregion layer ignores
}

[System.Serializable]
public struct IgnoredLayers
{
    public string firstLayer, secondLayer;

    /// <summary>
    /// Applies and ignore the layers.
    /// </summary>
    public void Apply()
    {
        int layer1 = LayerMask.NameToLayer(firstLayer);
        int layer2 = LayerMask.NameToLayer(secondLayer);
        Physics2D.IgnoreLayerCollision(layer1, layer2, true);
    }

    /// <summary>
    /// Revert the ignore between the two layers.
    /// </summary>
    public void Revert()
    {
        int layer1 = LayerMask.NameToLayer(firstLayer);
        int layer2 = LayerMask.NameToLayer(secondLayer);
        Physics2D.IgnoreLayerCollision(layer1, layer2, false);
    }
}