using System;
using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [Header("Tool Required")]
    [Tooltip("Which hotbar item type can damage this object.")]
    [SerializeField] private ItemAnimationType requiredTool = ItemAnimationType.Axe;

    public ItemAnimationType RequiredTool => requiredTool;

    [Header("Resistance")]
    [Tooltip("Number of hits required to destroy this object.")]
    [Min(1)]
    [SerializeField] private int hitsToDestroy = 3;

    [Header("Animation")]
    [Tooltip("Animator on this object (optional).")]
    [SerializeField] private Animator animator;
    [Tooltip("Animator trigger name played on each hit. Leave empty to skip.")]
    [SerializeField] private string hitTrigger = "Hit";
    [Tooltip("Animator trigger name played on destruction. Leave empty to skip.")]
    [SerializeField] private string destroyTrigger = "Destroy";
    [Tooltip("Seconds to wait after the destroy animation before removing the GameObject.")]
    [Min(0f)]
    [SerializeField] private float destroyDelay = 0.5f;

    [Header("Loot")]
    [SerializeField] private LootEntry[] lootTable = Array.Empty<LootEntry>();
    [Tooltip("Max scatter radius for dropped items.")]
    [Min(0f)]
    [SerializeField] private float scatterRadius = 0.35f;
    [SerializeField] private int spriteSortingOrder = 5;

    private int hitsLeft;
    private bool destroyed;

    private void Awake()
    {
        hitsLeft = hitsToDestroy;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void TakeHit()
    {
        if (destroyed)
            return;

        hitsLeft--;

        if (hitsLeft > 0)
        {
            PlayTrigger(hitTrigger);
            return;
        }

        DestroyObject();
    }

    private void DestroyObject()
    {
        destroyed = true;
        DropLoot();
        PlayTrigger(destroyTrigger);

        // disable collider so player can walk through the stump
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        UnityEngine.Object.Destroy(gameObject, destroyDelay);
    }

    private void PlayTrigger(string triggerName)
    {
        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            int hash = Animator.StringToHash(triggerName);
            if (animator.HasState(0, hash) || HasParameter(triggerName))
                animator.SetTrigger(triggerName);
        }
    }

    private bool HasParameter(string paramName)
    {
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.name == paramName) return true;
        return false;
    }

    private void DropLoot()
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
                SpawnDrop(entry, origin + offset);
            }
        }
    }

    private void SpawnDrop(LootEntry entry, Vector2 position)
    {
        GameObject go = new GameObject("Drop_" + entry.itemName);
        go.transform.position = position;

        if (entry.itemIcon != null)
        {
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = entry.itemIcon;
            sr.sortingOrder = spriteSortingOrder;
        }

        PickupItem pickup = go.AddComponent<PickupItem>();
        pickup.SetData(entry.itemName, entry.itemIcon, entry.effectType, entry.animationType,
                       entry.effectValue, 1f, Mathf.Max(1, entry.maxStack));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.3f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, scatterRadius);
    }
}
