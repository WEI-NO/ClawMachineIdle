using System;
using UnityEngine;

public class ArmTrigger : MonoBehaviour
{
    public Action<GameObject> OnPrizeDetected;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Prizes"))
        {
            OnPrizeDetected?.Invoke(collision.gameObject);
        }
    }
}
