using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrizeDetector : MonoBehaviour
{
    private ClawObject parentClaw;
    public int containedCount = 0;
    public int containThreshold = 2;

    public float distanceThreshold = 1.0f;
    public LayerMask groundLayer;

    public List<BasePrize> grabbedObj;
    [SerializeField] private GameObject claimEffect;
    [SerializeField] private float claimDelay = 0.1f;
    private Coroutine prizeClaimCoroutine = null;

    private void Awake()
    {
        parentClaw = GetComponentInParent<ClawObject>();
        parentClaw.OnGrabbed += OnGrabbed;
        parentClaw.OnClaimPrize += ClaimPrize;
    }

    private void Update()
    {
        HeightDetection();
    }

    private void ClaimPrize()
    {
        if (prizeClaimCoroutine != null)
        {
            StopCoroutine(prizeClaimCoroutine);
        }
        prizeClaimCoroutine = StartCoroutine(ClaimPrizeCoroutine());
    }

    private IEnumerator ClaimPrizeCoroutine()
    {
        for (int i = grabbedObj.Count - 1; i >= 0; i--)
        {
            if (grabbedObj[i])
            {
                if (claimEffect)
                {
                    Instantiate(claimEffect, grabbedObj[i].transform.position, Quaternion.identity);
                }
                grabbedObj[i].PrizeClaim();
            }
            yield return new WaitForSeconds(claimDelay);
        }

        grabbedObj.Clear();
    }

    private void OnGrabbed()
    {
        LockPrizes();
        parentClaw.clawC.SetClawState(true);
    }

    private void HeightDetection()
    {
        if (!parentClaw || !parentClaw.downSequence)
        {
            return;
        }
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, float.PositiveInfinity, groundLayer);
        if (hit.collider)
        {
            float dist = Vector2.Distance(hit.point , (Vector2)transform.position);
            if (dist <= distanceThreshold)
            {
                parentClaw.ExpediteGrab();
            }
        }
    }

    private void LockPrizes()
    {
        foreach (var obj in grabbedObj)
        {
            var rb = obj.GetComponent<Rigidbody2D>();
            if (rb)
            {
                rb.linearVelocity = Vector2.zero; // Stop motion
                rb.angularVelocity = 0f;
                rb.gravityScale = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic; // <-- Key line
            }

            var col = obj.GetComponent<Collider2D>();
            if (col)
            {
                col.isTrigger = true;
            }

            obj.transform.SetParent(transform, true); // Follow parent
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Prize"))
        {
            BasePrize prize = collision.gameObject.GetComponent<BasePrize>();
            grabbedObj.Add(prize);
            containedCount++;
            if (containedCount >= containThreshold)
            {
                parentClaw.ExpediteGrab();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Prize"))
        {
            BasePrize prize = collision.gameObject.GetComponent<BasePrize>();
            grabbedObj.Remove(prize);
            containedCount = Mathf.Clamp(containedCount - 1, 0, containedCount);

        }
    }
}
