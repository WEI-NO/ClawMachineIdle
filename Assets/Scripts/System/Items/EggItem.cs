using UnityEngine;

[CreateAssetMenu(fileName = "Placeable Item", menuName = "Bubble Claw/Egg Item")]
public class EggItem : BaseItem
{
    public SO_EggPrizeEntries lootTable;
    public float hatchTime_s = 300.0f;
}
