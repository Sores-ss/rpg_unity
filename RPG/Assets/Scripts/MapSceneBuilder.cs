using UnityEngine;
using UnityEngine.Tilemaps;

public class MapSceneBuilder : MonoBehaviour
{
    [Header("Multi-level setup")]
    [Tooltip("Number of height levels to create when using 'Build Multi-Level Map Layers'.")]
    [Min(2)]
    [SerializeField] private int levelCount = 2;

    [ContextMenu("Build Map Layers (single level)")]
    private void BuildMapLayers()
    {
        if (FindFirstObjectByType<Grid>() != null)
        {
            Debug.LogWarning("MapSceneBuilder: a Grid already exists in the scene. Remove it first or delete this component.");
            return;
        }

        GameObject mapRoot = new GameObject("Map");
        mapRoot.AddComponent<Grid>().cellSize = Vector3.one;

        CreateLayer(mapRoot.transform, "Ground",  -10, collision: false);
        CreateLayer(mapRoot.transform, "Walls",    -5, collision: true);
        CreateLayer(mapRoot.transform, "Props",     5, collision: false);
        CreateLayer(mapRoot.transform, "Overhead", 20, collision: false);

        Debug.Log(
            "Single-level map created.\n" +
            "Window > 2D > Tile Palette to start painting.\n" +
            "  Ground = floor/terrain  |  Walls = solid obstacles\n" +
            "  Props = deco no collision  |  Overhead = renders above player\n" +
            "Delete the MapSceneBuilder GameObject when done."
        );
    }

    [ContextMenu("Build Multi-Level Map Layers")]
    private void BuildMultiLevelMapLayers()
    {
        if (FindFirstObjectByType<Grid>() != null)
        {
            Debug.LogWarning("MapSceneBuilder: a Grid already exists in the scene. Remove it first.");
            return;
        }

        GameObject mapRoot = new GameObject("Map");
        mapRoot.AddComponent<Grid>().cellSize = Vector3.one;

        for (int i = 0; i < levelCount; i++)
        {
            GameObject levelParent = new GameObject("Level_" + i);
            levelParent.transform.SetParent(mapRoot.transform, false);

            TilemapRenderer groundR   = CreateLayer(levelParent.transform, "Ground",   -10, collision: false);
            TilemapRenderer wallsR    = CreateLayer(levelParent.transform, "Walls",     -5, collision: true);
            TilemapRenderer propsR    = CreateLayer(levelParent.transform, "Props",      5, collision: false);
            TilemapRenderer overheadR = CreateLayer(levelParent.transform, "Overhead",  20, collision: false);

            HeightLevelGroup group = levelParent.AddComponent<HeightLevelGroup>();
            group.Initialize(i, groundR, wallsR, propsR, overheadR);
        }

        Debug.Log(
            $"{levelCount} height levels created (Level_0 = ground, Level_1 = elevated...).\n" +
            "Add PlayerLevel component to your player.\n" +
            "Place StairsTrigger zones at staircase/bridge entry points.\n" +
            "Paint each level's tilemaps independently with the Tile Palette.\n" +
            "Delete the MapSceneBuilder GameObject when done."
        );
    }

    private static TilemapRenderer CreateLayer(Transform parent, string layerName, int sortingOrder, bool collision)
    {
        GameObject go = new GameObject(layerName);
        go.transform.SetParent(parent, false);

        go.AddComponent<Tilemap>();

        TilemapRenderer tilemapRenderer = go.AddComponent<TilemapRenderer>();
        tilemapRenderer.sortingOrder = sortingOrder;

        if (collision)
        {
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            TilemapCollider2D col = go.AddComponent<TilemapCollider2D>();
            col.compositeOperation = Collider2D.CompositeOperation.Merge;

            CompositeCollider2D comp = go.AddComponent<CompositeCollider2D>();
            comp.geometryType = CompositeCollider2D.GeometryType.Polygons;
        }

        return tilemapRenderer;
    }
}
