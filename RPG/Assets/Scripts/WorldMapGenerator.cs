using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldMapGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap wallTilemap;

    [Header("Default TileSets")]
    [Tooltip("Floor tiles used for corridors and zones with no specific TileSet assigned.")]
    [SerializeField] private TileSet defaultFloorTileSet;
    [Tooltip("Wall tiles used around all floor areas.")]
    [SerializeField] private TileSet wallTileSet;

    [Header("Zone Floor TileSets (optional — uses default if empty)")]
    [SerializeField] private TileSet hubFloorTileSet;
    [SerializeField] private TileSet northFloorTileSet;
    [SerializeField] private TileSet southFloorTileSet;
    [SerializeField] private TileSet eastFloorTileSet;
    [SerializeField] private TileSet westFloorTileSet;

    [Header("Hub")]
    [Min(6)]
    [SerializeField] private int hubSize = 20;

    [Header("Zones")]
    [Min(6)]
    [SerializeField] private int zoneSize = 24;

    [Header("Corridors")]
    [Min(4)]
    [SerializeField] private int corridorLength = 14;
    [Min(2)]
    [SerializeField] private int corridorWidth = 4;

    [Header("Player")]
    [SerializeField] private Transform player;

    [ContextMenu("Generate (once — then save the scene)")]
    public void Generate()
    {
        if (!ValidateReferences())
            return;

        Dictionary<Vector2Int, TileBase> floorMap = BuildFloorMap();
        PaintTiles(floorMap);
        SetupWallCollision();
        SpawnPlayer();
    }

    private bool ValidateReferences()
    {
        if (groundTilemap == null)   { Debug.LogError("WorldMapGenerator: Ground Tilemap not assigned.", this); return false; }
        if (wallTilemap == null)     { Debug.LogError("WorldMapGenerator: Wall Tilemap not assigned.", this);   return false; }
        if (defaultFloorTileSet == null || defaultFloorTileSet.IsEmpty)
        {
            Debug.LogError("WorldMapGenerator: Default Floor TileSet is empty — assign at least one tile.", this);
            return false;
        }
        if (wallTileSet == null || wallTileSet.IsEmpty)
        {
            Debug.LogError("WorldMapGenerator: Wall TileSet is empty — assign at least one tile.", this);
            return false;
        }
        return true;
    }

    private Dictionary<Vector2Int, TileBase> BuildFloorMap()
    {
        int hh = hubSize / 2;
        int hz = zoneSize / 2;
        int hc = corridorWidth / 2;

        TileSet Resolve(TileSet ts) => (ts != null && !ts.IsEmpty) ? ts : defaultFloorTileSet;

        var map = new Dictionary<Vector2Int, TileBase>();

        FillRect(map, new RectInt(-hh, -hh, hubSize, hubSize),                                    Resolve(hubFloorTileSet));
        FillRect(map, new RectInt(-hc,  hh,  corridorWidth, corridorLength),                      defaultFloorTileSet);
        FillRect(map, new RectInt(-hz,  hh + corridorLength, zoneSize, zoneSize),                 Resolve(northFloorTileSet));
        FillRect(map, new RectInt(-hc, -(hh + corridorLength), corridorWidth, corridorLength),    defaultFloorTileSet);
        FillRect(map, new RectInt(-hz, -(hh + corridorLength + zoneSize), zoneSize, zoneSize),    Resolve(southFloorTileSet));
        FillRect(map, new RectInt( hh, -hc, corridorLength, corridorWidth),                       defaultFloorTileSet);
        FillRect(map, new RectInt( hh + corridorLength, -hz, zoneSize, zoneSize),                 Resolve(eastFloorTileSet));
        FillRect(map, new RectInt(-(hh + corridorLength), -hc, corridorLength, corridorWidth),    defaultFloorTileSet);
        FillRect(map, new RectInt(-(hh + corridorLength + zoneSize), -hz, zoneSize, zoneSize),    Resolve(westFloorTileSet));

        return map;
    }

    private static void FillRect(Dictionary<Vector2Int, TileBase> map, RectInt rect, TileSet tileSet)
    {
        for (int x = rect.xMin; x < rect.xMax; x++)
            for (int y = rect.yMin; y < rect.yMax; y++)
                map[new Vector2Int(x, y)] = tileSet.PickRandom();
    }

    private void PaintTiles(Dictionary<Vector2Int, TileBase> floorMap)
    {
        groundTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        foreach (KeyValuePair<Vector2Int, TileBase> kv in floorMap)
            groundTilemap.SetTile(new Vector3Int(kv.Key.x, kv.Key.y, 0), kv.Value);

        foreach (Vector2Int cell in floorMap.Keys)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    Vector2Int neighbor = new Vector2Int(cell.x + dx, cell.y + dy);
                    if (!floorMap.ContainsKey(neighbor))
                        wallTilemap.SetTile(new Vector3Int(neighbor.x, neighbor.y, 0), wallTileSet.PickRandom());
                }
            }
        }
    }

    private void SetupWallCollision()
    {
        TilemapCollider2D col = wallTilemap.GetComponent<TilemapCollider2D>();
        if (col == null) col = wallTilemap.gameObject.AddComponent<TilemapCollider2D>();
        col.compositeOperation = Collider2D.CompositeOperation.Merge;

        Rigidbody2D rb = wallTilemap.GetComponent<Rigidbody2D>();
        if (rb == null) rb = wallTilemap.gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        CompositeCollider2D comp = wallTilemap.GetComponent<CompositeCollider2D>();
        if (comp == null) comp = wallTilemap.gameObject.AddComponent<CompositeCollider2D>();
        comp.geometryType = CompositeCollider2D.GeometryType.Polygons;
    }

    private void SpawnPlayer()
    {
        if (player == null)
            return;

        player.position = groundTilemap.GetCellCenterWorld(Vector3Int.zero);
    }

    private void OnDrawGizmosSelected()
    {
        int hh = hubSize / 2;
        int hz = zoneSize / 2;

        DrawZoneGizmo(new RectInt(-hh, -hh, hubSize, hubSize),                                     Color.white,   "Hub");
        DrawZoneGizmo(new RectInt(-hz, hh + corridorLength, zoneSize, zoneSize),                   Color.cyan,    "Nord");
        DrawZoneGizmo(new RectInt(-hz, -(hh + corridorLength + zoneSize), zoneSize, zoneSize),     Color.red,     "Sud");
        DrawZoneGizmo(new RectInt(hh + corridorLength, -hz, zoneSize, zoneSize),                   Color.green,   "Est");
        DrawZoneGizmo(new RectInt(-(hh + corridorLength + zoneSize), -hz, zoneSize, zoneSize),     Color.magenta, "Ouest");
    }

    private static void DrawZoneGizmo(RectInt rect, Color color, string label)
    {
        Gizmos.color = new Color(color.r, color.g, color.b, 0.25f);
        Vector3 center = new Vector3(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f, 0f);
        Vector3 size   = new Vector3(rect.width, rect.height, 0f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = color;
        Gizmos.DrawWireCube(center, size);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(center, label);
#endif
    }
}
