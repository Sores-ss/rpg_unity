using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public struct SpawnEntry
    {
        [Tooltip("Enemy prefab to instantiate.")]
        public GameObject prefab;
        [Tooltip("How many of this enemy to spawn.")]
        [Min(1)] public int count;
        [Tooltip("Custom spawn origin. Leave empty to use this spawner's position.")]
        public Transform spawnPoint;
        [Tooltip("Random scatter radius around the spawn origin (0 = exact position).")]
        [Min(0f)] public float scatterRadius;
    }

    [Header("Enemies")]
    [SerializeField] private SpawnEntry[] entries = Array.Empty<SpawnEntry>();

    [Header("Settings")]
    [Tooltip("Spawn all enemies when the scene starts.")]
    [SerializeField] private bool spawnOnStart = true;

    private void Start()
    {
        if (spawnOnStart)
            SpawnAll();
    }

    [ContextMenu("Spawn All")]
    public void SpawnAll()
    {
        foreach (SpawnEntry entry in entries)
            Spawn(entry);
    }

    private void Spawn(SpawnEntry entry)
    {
        if (entry.prefab == null || entry.count <= 0)
            return;

        Vector2 origin = entry.spawnPoint != null
            ? (Vector2)entry.spawnPoint.position
            : (Vector2)transform.position;

        for (int i = 0; i < entry.count; i++)
        {
            Vector2 offset = entry.scatterRadius > 0f
                ? UnityEngine.Random.insideUnitCircle * entry.scatterRadius
                : Vector2.zero;

            Instantiate(entry.prefab, (Vector3)(origin + offset), Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.6f);
        foreach (SpawnEntry entry in entries)
        {
            Vector3 origin = entry.spawnPoint != null ? entry.spawnPoint.position : transform.position;
            Gizmos.DrawWireSphere(origin, Mathf.Max(entry.scatterRadius, 0.25f));
        }
    }
}
