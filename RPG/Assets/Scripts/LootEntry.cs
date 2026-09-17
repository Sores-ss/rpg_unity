using System;
using UnityEngine;

[Serializable]
public struct LootEntry
{
    [Tooltip("Name added to the player's inventory.")]
    public string itemName;
    [Tooltip("Icon displayed in the world and in the inventory.")]
    public Sprite itemIcon;
    [Tooltip("Effect applied when the item is used.")]
    public ItemEffectType effectType;
    [Tooltip("Animation set used when this item is held by the player.")]
    public ItemAnimationType animationType;
    [Tooltip("Value passed to the effect (e.g. heal amount or gold value).")]
    [Min(0)] public int effectValue;
    [Tooltip("Maximum number of this item per inventory slot (1 = non-stackable).")]
    [Min(1)] public int maxStack;
    [Tooltip("Probability to drop (0 = never, 1 = always).")]
    [Range(0f, 1f)] public float dropChance;
    [Tooltip("Minimum number of copies dropped when this entry triggers.")]
    [Min(1)] public int minCount;
    [Tooltip("Maximum number of copies dropped when this entry triggers.")]
    [Min(1)] public int maxCount;
}
