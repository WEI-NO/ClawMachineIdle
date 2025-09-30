using CustomLibrary.References;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using CMT = ClawMachineThemeController;

public class PrizeDumper : MonoBehaviour
{
    [SerializeField] private CraneStickController craneStick;

    public bool InSequence = false;

    public List<BasePrize> prizeBag = new List<BasePrize>();

    //[SerializeField] private PrizeSetting prizeSetting;
    [SerializeField] private Transform prizeContainer;

    [SerializeField] private float prizeDumpCooldown = 0.3f;

    [SerializeField] private float oldPrizeDeleteWaitTime = 3.0f;
    [SerializeField] private Transform table;
    [SerializeField] private RefreshingUI refreshingUI;


    [SerializeField] private float prizeDumpForce = 5f;      // Magnitude of force
    [SerializeField] private float minAngle = -45f;          // In degrees, left-down
    [SerializeField] private float maxAngle = 45f;           // In degrees, right-down

    [Header("Prize Properties")]
    [SerializeField] private SO_ClawMachinePrizeEntries currentPrizeEntry;
    public Vector2Int DropAmountRange = new Vector2Int(15, 20);
    public int DropAmountInterval = 2;

    [Header("Themes Properties")]
    [SerializeField] private List<SO_ClawMachinePrizeEntries> themedEntries;

    [Header("Cost Properties")]
    public bool isFree = false;

    private void Start()
    {
        if (CMT.Instance)
        {
            CMT.Instance.OnThemeChange += OnThemeChange;
        }
        OnThemeChange(CMT.Instance.CurrentTheme);
    }

    private void OnThemeChange(CM_Theme theme)
    {
        int themeIndex = theme.ToInt();
        if (themedEntries != null && themedEntries.Count > themeIndex)
        {
            currentPrizeEntry = themedEntries[themeIndex];
        }
    }

    // Initiates the prize refresh main sequence
    public void StartRefreshPrize()
    {
        if (InSequence)
        {
            return;
        }
        StartCoroutine(RefreshPrizeSequence());
        InSequence = true;
    }

    // Main sequence for prize refresh
    private IEnumerator RefreshPrizeSequence()
    {
        craneStick.SetActive(false);

        if (refreshingUI)
        {
            refreshingUI.Activate();
        }

        yield return DeleteAllPrize();


        yield return RandomizePrizeBag();

        yield return DumpPrizeBag();

        yield return new WaitForSeconds(1.0f);

        craneStick.SetActive(true);

        if (refreshingUI)
        {
            refreshingUI.Deactivate();
        }

        InSequence = false;
    }
    // Deletes existing prizes
    private IEnumerator DeleteAllPrize()
    {
        Collider2D tableCol = null;
        if (table && table.GetComponent<Collider2D>() is Collider2D col)
        {
            tableCol = col;
            tableCol.isTrigger = true;
        }

        yield return new WaitForSeconds(oldPrizeDeleteWaitTime);

        for (int i = prizeContainer.childCount - 1; i >= 0; i--)
        {
            if (prizeContainer.GetChild(i) != null)
            {
                Destroy(prizeContainer.GetChild(i).gameObject);
            }
            yield return null;
        }

        if (tableCol)
        {
            tableCol.isTrigger = false;
        }
    }
    // Randomizes the loot bag / prize pool
    private IEnumerator RandomizePrizeBag()
    {
        int intervalCount = Mathf.CeilToInt((DropAmountRange.y - DropAmountRange.x) / (float)DropAmountInterval);
        int randomizedAmount = UnityEngine.Random.Range(0, intervalCount);
        int dropAmount = DropAmountRange.x + randomizedAmount * DropAmountInterval;

        int dropSelected = 0;

        while (dropSelected < dropAmount)
        {
            var db = MainDatabase.Instance.DB_Prize;
            var prizeData = currentPrizeEntry.RollPrize().prize;

            BasePrize newPrize = null;
            yield return db.LoadAssetCoroutine(prizeData,
                (i) =>
                {
                    if (i == null) return;
                    newPrize = Instantiate(i);
                    if (newPrize) newPrize.gameObject.SetActive(false);
                });

            if (newPrize == null)
            {
                if (prizeData)
                {
                    print("Failed to find item: " + prizeData.ItemName);
                }
                else
                {
                    print("No Random Item Selected, Skipping");
                }
                continue;
            }

            newPrize.gameObject.SetActive(false);
            newPrize.transform.SetParent(prizeContainer);
            newPrize.transform.position = transform.position;
            prizeBag.Add(newPrize);
            dropSelected++;
            yield return null;
        }
        yield break;
    }
    // Dumps physical prizes into the machine
    private IEnumerator DumpPrizeBag()
    {
        for (int i = 0; i < prizeBag.Count; i++)
        {
            if (prizeBag[i] == null)
            {
                continue;
            }

            // Activate prize
            prizeBag[i].gameObject.SetActive(true);

            // Apply random downward force
            Rigidbody2D rb = prizeBag[i].GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Angle in degrees, y+ is downward
                float angle = UnityEngine.Random.Range(minAngle, maxAngle);
                float angleRad = angle * Mathf.Deg2Rad;

                // Down is (0,1) in screen space
                Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                //Vector2 direction = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad));
                // Mathf.Cos(angleRad) gives y component (down), Sin is x (left/right)

                rb.AddForce(direction * prizeDumpForce, ForceMode2D.Impulse);
            }

            yield return new WaitForSeconds(prizeDumpCooldown);
        }
        prizeBag.Clear();
    }
}