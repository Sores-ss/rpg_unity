using UnityEngine;
using UnityEngine.EventSystems;

public class HotbarSlot : MonoBehaviour, IPointerClickHandler
{
    public int slotIndex;
    public InventoryUI inventoryUI;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryUI == null)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (eventData.clickCount >= 2)
        {
            inventoryUI.TryTransferSlotToChest(slotIndex);
            return;
        }

        inventoryUI.SelectSlot(slotIndex);
    }
}
