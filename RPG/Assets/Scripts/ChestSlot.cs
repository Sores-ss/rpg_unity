using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChestSlot : MonoBehaviour, IPointerClickHandler
{
    public int slotIndex;
    public ChestSystem chestSystem;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (chestSystem == null)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (eventData.clickCount >= 2)
        {
            chestSystem.TransferChestSlotToPlayer(slotIndex);
            return;
        }

        chestSystem.OnSlotClicked(slotIndex);
    }
}
