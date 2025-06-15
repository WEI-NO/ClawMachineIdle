using System;
using UnityEngine;

[Serializable]
public class PrizeEntry : ItemEntry<PrizeItem> { }

[CreateAssetMenu(menuName = "Game/Claw Machine Prize List")]
public class SO_ClawMachinePrizeEntries : SO_BasePrizeEntries<PrizeEntry, PrizeItem> { }