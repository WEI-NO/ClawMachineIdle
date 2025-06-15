using System;
using UnityEngine;

[Serializable]
public class EggEntry : ItemEntry<BaseItem> { }

[CreateAssetMenu(menuName = "Game/Egg Prize List")]
public class SO_EggPrizeEntries : SO_BasePrizeEntries<EggEntry, BaseItem> { }