using UnityEngine;

public class Prize_RangeQuantity : BasePrize
{
    [SerializeField] private Vector2Int quantityRange;

    protected override void OnStart()
    {
        int quantity = Random.Range(quantityRange.x, quantityRange.y);
        SetQuantity(quantity);
    }

}
