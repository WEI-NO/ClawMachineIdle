using UnityEngine;

public class PrizeDetector : MonoBehaviour
{
    private ClawObject parentClaw;
    public int containedCount = 0;
    public int containThreshold = 2;

    private void Awake()
    {
        parentClaw = GetComponentInParent<ClawObject>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Prize"))
        {
            containedCount++;
            if (containedCount >= 2)
            {
                parentClaw.ExpediteGrab();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Prize"))
        {
            containedCount--;
        }
    }
}
