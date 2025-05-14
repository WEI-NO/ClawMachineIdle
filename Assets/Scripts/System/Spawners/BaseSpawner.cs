using CustomLibrary.References;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSpawner<T> : MonoBehaviour
{
    public List<T> containedObjets;

    public virtual T Get(int id)
    {
        return containedObjets[id];
    }

}
