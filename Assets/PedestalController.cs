using UnityEngine;

public class PedestalController : MonoBehaviour
{
    Animator pedestalAnim;
    private void Awake()
    {
        pedestalAnim = GetComponent<Animator>();
    }

    public void ClaimFunction()
    {
        var controller = IncubationController.Instance;
        var index = controller.GetFirstInQueue_Index(false);
        if (index < 0) return;
        var container = controller.GetEggFromQueue(index);
        if (container == null || !container.Done()) return;

        var prizeItem = container.heldEgg.lootTable.RollPrize().prize;
        PlayerInventory.Instance.GiveItem(prizeItem, 1);
        ItemPopup.Instance.AddItem(prizeItem);
        controller.RemoveAt(index);
        pedestalAnim.ResetTrigger("Claim");
    }
}
