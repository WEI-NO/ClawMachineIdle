using CustomLibrary.References;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PrizeDumper : MonoBehaviour
{
    [SerializeField] private CraneStickController craneStick;

    public bool InSequence = false;

    public List<BasePrize> prizeBag = new List<BasePrize>();
    [SerializeField] private PrizeSetting prizeSetting;
    [SerializeField] private Transform prizeContainer;

    [SerializeField] private float prizeDumpCooldown = 0.3f;

    [SerializeField] private float oldPrizeDeleteWaitTime = 3.0f;
    [SerializeField] private Transform table;
    [SerializeField] private RefreshingUI refreshingUI;


    [SerializeField] private float prizeDumpForce = 5f;      // Magnitude of force
    [SerializeField] private float minAngle = -45f;          // In degrees, left-down
    [SerializeField] private float maxAngle = 45f;           // In degrees, right-down
    private void Start()
    {

    }

    public void StartRefreshPrize()
    {
        if (InSequence)
        {
            return;
        }
        StartCoroutine(RefreshPrizeSequence());
        InSequence = true;
    }

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

    private IEnumerator RefreshPrizeSequence()
    {
        craneStick.SetActive(false);

        if (refreshingUI)
        {
            refreshingUI.Activate();
        }

        yield return DeleteAllPrize();


        yield return RandomizePrizeBag(prizeSetting);

        yield return DumpPrizeBag();

        yield return new WaitForSeconds(1.0f);

        craneStick.SetActive(true);

        if (refreshingUI)
        {
            refreshingUI.Deactivate();
        }

        InSequence = false;
    }

    private IEnumerator RandomizePrizeBag(PrizeSetting setting)
    {
        var odds = setting.odds.GetOdds();
        int intervalCount = Mathf.CeilToInt((setting.DropAmountRange.y - setting.DropAmountRange.x) / (float)setting.DropAmountInterval);
        int randomizedAmount = UnityEngine.Random.Range(0, intervalCount);
        int dropAmount = setting.DropAmountRange.x + randomizedAmount * setting.DropAmountInterval;

        int dropSelected = 0;

        while (dropSelected < dropAmount)
        {
            float rarityRoll = UnityEngine.Random.Range(0.0f, 1.0f);
            int rarity = 0;
            for (int i = odds.Count - 1; i >= 0; i--)
            {
                if (rarityRoll >= (1.0f - odds[i]))
                {
                    rarity = i;
                    break;
                }
            }

            var db = MainDatabase.Instance.DB_Prize;
            var prizeData = db.GetRandomItemByRarity((ItemRarity)rarity);

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
                } else
                {
                    print("No Random Item Selected, Skipping");
                }
                continue;
            }

            if (newPrize.isEgg && !prizeSetting.odds.RollEggChance())
            {
                print("Deleted one egg;");
                Destroy(newPrize.gameObject);
                continue;
            }
            print("Loaded item: " + prizeData.ItemName);

            newPrize.gameObject.SetActive(false);
            newPrize.transform.SetParent(prizeContainer);
            newPrize.transform.position = transform.position;
            prizeBag.Add(newPrize);
            dropSelected++;
            yield return null;
        }
        yield break;
    }



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

[System.Serializable]
public class PrizeSetting
{
    public Vector2Int DropAmountRange = new Vector2Int(5, 10);
    public int DropAmountInterval = 1; // Determines the range between dropAmountRange, so interval of 1 gets 5, 6, 7 etc. interval of 2 gets 5, 7, 9,
    public PrizeOdds odds;

    public PrizeSetting()
    {
        odds.GetOdds();
    }
}

[System.Serializable]
public struct PrizeOdds
{
    public bool initialized;
    public bool valid;
    private bool clean;
    public float eggChance;
    public List<int> oddCounts;
    public List<float> _odds;
    public List<float> odds { 
        get
        {
            CalculateOdds(oddCounts);
            if (!valid) return null;
            return _odds;
        }
        private set { } }

    public List<float> GetOdds()
    {
        return odds;
    }

    private void CalculateOdds(List<int> count)
    {
        //if (clean) return;
        if (count == null) {
            valid = false;
            return;
        }

        valid = true;
        if (count.Count != ItemRarity.Count.ToInt())
        {
            clean = false;
            valid = false;
            return;
        }
        _odds = new List<float>();
        int sum = 0;
        foreach (int i in count)
        {
            sum += i;
        }

        for (int i = 0; i < count.Count; i++)
        {
            if (sum == 0)
            {
                _odds.Add(1.0f / count.Count);
            } else
            {
                _odds.Add((float)count[i] / sum);
            }
        }
        clean = true;
    }

    public bool RollEggChance()
    {
        float chance = Mathf.Clamp(eggChance, 0, 1.0f);

        float roll = UnityEngine.Random.Range(0.0f, 1.0f);

        return roll <= chance;
    }

}