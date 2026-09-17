using System;
using UnityEngine;

public enum ItemEffectType
{
    None = 0,
    Heal = 1,
    Gold = 2,
}

[Serializable]
public struct InventoryItemData
{
    public string           itemName;
    public Sprite           itemIcon;
    public ItemEffectType   effectType;
    public ItemAnimationType animationType;
    public int              effectValue;
    public int              quantity;
    public int              maxStack;

    public readonly int EffectiveQuantity => quantity < 1 ? 1 : quantity;
    public readonly int EffectiveMaxStack => maxStack < 1 ? 1 : maxStack;
}

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Data")]
    [Tooltip("Maximum number of items storable in inventory.")]
    [Min(1)]
    [SerializeField] private int maxSlots = 24;
    [SerializeField] private InventoryItemData[] inventoryArray = Array.Empty<InventoryItemData>();
    [SerializeField] private int itemCount;

    public static InventoryManager Instance { get; private set; }

    public event Action InventoryChanged;

    public int ItemCount => itemCount;
    public InventoryItemData[] InventoryArray => inventoryArray;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (inventoryArray == null || inventoryArray.Length != maxSlots)
            inventoryArray = new InventoryItemData[maxSlots];

        itemCount = Mathf.Clamp(itemCount, 0, inventoryArray.Length);
    }

    public bool AddItem(string itemName, Sprite itemIcon, ItemEffectType effectType = ItemEffectType.None, ItemAnimationType animationType = ItemAnimationType.None, int effectValue = 0, int quantity = 1, int maxStack = 1)
    {
        if (string.IsNullOrWhiteSpace(itemName) || quantity <= 0)
            return false;

        maxStack = Mathf.Max(1, maxStack);
        int remaining = quantity;
        bool changed = false;

        for (int i = 0; i < itemCount && remaining > 0; i++)
        {
            if (inventoryArray[i].itemName != itemName)
                continue;

            int space = inventoryArray[i].EffectiveMaxStack - inventoryArray[i].EffectiveQuantity;
            if (space <= 0)
                continue;

            int toAdd = Mathf.Min(space, remaining);
            inventoryArray[i].quantity = inventoryArray[i].EffectiveQuantity + toAdd;
            remaining -= toAdd;
            changed = true;
        }

        while (remaining > 0 && itemCount < inventoryArray.Length)
        {
            int toAdd = Mathf.Min(remaining, maxStack);
            inventoryArray[itemCount++] = new InventoryItemData
            {
                itemName      = itemName,
                itemIcon      = itemIcon,
                effectType    = effectType,
                animationType = animationType,
                effectValue   = effectValue,
                quantity      = toAdd,
                maxStack      = maxStack
            };
            remaining -= toAdd;
            changed = true;
        }

        if (changed)
            InventoryChanged?.Invoke();

        return remaining == 0;
    }

    public bool TryGetItem(int index, out InventoryItemData itemData)
    {
        if (index < 0 || index >= itemCount)
        {
            itemData = default;
            return false;
        }

        itemData = inventoryArray[index];
        return true;
    }

    public bool UseItemAt(int index)
    {
        if (index < 0 || index >= itemCount)
            return false;

        if (inventoryArray[index].EffectiveQuantity > 1)
        {
            inventoryArray[index].quantity = inventoryArray[index].EffectiveQuantity - 1;
            InventoryChanged?.Invoke();
            return true;
        }

        return RemoveSlotAt(index);
    }

    public bool RemoveSlotAt(int index)
    {
        if (index < 0 || index >= itemCount)
            return false;

        for (int i = index; i < itemCount - 1; i++)
            inventoryArray[i] = inventoryArray[i + 1];

        inventoryArray[itemCount - 1] = default;
        itemCount--;

        InventoryChanged?.Invoke();
        return true;
    }
}
