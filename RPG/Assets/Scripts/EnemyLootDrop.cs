using System;
using UnityEngine;

[RequireComponent(typeof(EnemyCombat))]
public class EnemyLootDrop : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("Enemy configuration asset. Required.")]
    [SerializeField] private EnemyData enemyData;

    [Header("Drop")]
    [Tooltip("Maximum radius around the enemy where items land.")]
    [Min(0f)]
    [SerializeField] private float scatterRadius = 0.35f;
    [Tooltip("Sorting order for spawned item sprites.")]
    [SerializeField] private int spriteSortingOrder = 5;

    private LootEntry[] lootTable = Array.Empty<LootEntry>();

    private void Awake()
    {
        if (enemyData == null)
        {
            Debug.LogError($"EnemyLootDrop on '{gameObject.name}' has no EnemyData assigned.", this);
            enabled = false;
            return;
        }

        lootTable = enemyData.lootTable ?? Array.Empty<LootEntry>();
        GetComponent<EnemyCombat>().Died += OnDied;
    }

    private void OnDied()
    {
        Vector2 origin = transform.position;

        for (int i = 0; i < lootTable.Length; i++)
        {
            LootEntry entry = lootTable[i];
            if (entry.dropChance <= 0f || string.IsNullOrWhiteSpace(entry.itemName))
                continue;

            if (UnityEngine.Random.value > entry.dropChance)
                continue;

            int count = UnityEngine.Random.Range(entry.minCount, Mathf.Max(entry.minCount, entry.maxCount) + 1);
            for (int j = 0; j < count; j++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * scatterRadius;
                SpawnWorldItem(entry, origin + offset, spriteSortingOrder);
            }
        }
    }

    private static void SpawnWorldItem(LootEntry entry, Vector2 position, int sortingOrder)
    {
        GameObject go = new GameObject("Loot_" + entry.itemName);
        go.transform.position = position;

        if (entry.itemIcon != null)
        {
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = entry.itemIcon;
            sr.sortingOrder = sortingOrder;
        }

        PickupItem pickup = go.AddComponent<PickupItem>();
        pickup.SetData(entry.itemName, entry.itemIcon, entry.effectType, entry.animationType, entry.effectValue, 1f, Mathf.Max(1, entry.maxStack));
    }
}
