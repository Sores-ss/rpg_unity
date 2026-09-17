using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    [Tooltip("Tilemap for the floor (no collision).")]
    [SerializeField] private Tilemap groundTilemap;
    [Tooltip("Tilemap for walls (collision is added automatically).")]
    [SerializeField] private Tilemap wallTilemap;

    [Header("Tiles")]
    [Tooltip("Tile painted on walkable floor cells.")]
    [SerializeField] private TileBase floorTile;
    [Tooltip("Tile painted on wall cells surrounding the floor.")]
    [SerializeField] private TileBase wallTile;

    [Header("Map Size")]
    [Tooltip("Total width of the map in tiles.")]
    [Min(20)]
    [SerializeField] private int mapWidth = 80;
    [Tooltip("Total height of the map in tiles.")]
    [Min(20)]
    [SerializeField] private int mapHeight = 60;

    [Header("Rooms")]
    [Tooltip("Number of rooms to place.")]
    [Min(2)]
    [SerializeField] private int roomCount = 12;
    [Tooltip("Minimum room width and height in tiles.")]
    [Min(3)]
    [SerializeField] private int minRoomSize = 5;
    [Tooltip("Maximum room width and height in tiles.")]
    [Min(3)]
    [SerializeField] private int maxRoomSize = 12;
    [Tooltip("How many random positions to try before giving up on a room.")]
    [Min(10)]
    [SerializeField] private int placementAttempts = 150;

    [Header("Corridors")]
    [Tooltip("Width of connecting corridors in tiles (1 = 1 tile wide).")]
    [Min(1)]
    [SerializeField] private int corridorWidth = 2;

    [Header("Player")]
    [Tooltip("Player transform to teleport to the spawn point. Optional.")]
    [SerializeField] private Transform player;

    [Header("Generation")]
    [Tooltip("Fixed seed for reproducible maps. Ignored if Random Seed On Start is true.")]
    [SerializeField] private int seed;
    [Tooltip("Use a random seed each time Generate() is called.")]
    [SerializeField] private bool randomSeedOnStart = true;

    private bool[,] floorMap;
    private readonly List<RectInt> rooms = new();

    public IReadOnlyList<RectInt> Rooms => rooms;
    public Vector3 PlayerSpawnWorldPosition { get; private set; }

    private void Start()
    {
        Generate();
    }

    [ContextMenu("Regenerate")]
    public void Generate()
    {
        if (!ValidateReferences())
            return;

        if (randomSeedOnStart)
            seed = Random.Range(int.MinValue, int.MaxValue);

        Random.InitState(seed);

        floorMap = new bool[mapWidth, mapHeight];
        rooms.Clear();

        PlaceRooms();
        ConnectRooms();
        PaintTiles();
        SetupWallCollision();
        SpawnPlayer();
    }

    private bool ValidateReferences()
    {
        if (groundTilemap == null) { Debug.LogError("DungeonGenerator: Ground Tilemap not assigned.", this); return false; }
        if (wallTilemap == null)   { Debug.LogError("DungeonGenerator: Wall Tilemap not assigned.", this);   return false; }
        if (floorTile == null)     { Debug.LogError("DungeonGenerator: Floor Tile not assigned.", this);     return false; }
        if (wallTile == null)      { Debug.LogError("DungeonGenerator: Wall Tile not assigned.", this);      return false; }
        return true;
    }

    private void PlaceRooms()
    {
        for (int attempt = 0; attempt < placementAttempts && rooms.Count < roomCount; attempt++)
        {
            int w = Random.Range(minRoomSize, maxRoomSize + 1);
            int h = Random.Range(minRoomSize, maxRoomSize + 1);
            int x = Random.Range(1, mapWidth  - w - 1);
            int y = Random.Range(1, mapHeight - h - 1);

            RectInt candidate = new RectInt(x, y, w, h);

            if (OverlapsExistingRoom(candidate))
                continue;

            rooms.Add(candidate);
            FillRect(candidate);
        }
    }

    private bool OverlapsExistingRoom(RectInt candidate)
    {
        foreach (RectInt existing in rooms)
        {
            RectInt padded = new RectInt(existing.xMin - 1, existing.yMin - 1,
                                         existing.width + 2, existing.height + 2);
            if (padded.Overlaps(candidate))
                return true;
        }
        return false;
    }

    private void FillRect(RectInt rect)
    {
        for (int x = rect.xMin; x < rect.xMax; x++)
            for (int y = rect.yMin; y < rect.yMax; y++)
                SetFloor(x, y);
    }

    private void ConnectRooms()
    {
        if (rooms.Count < 2)
            return;

        // Greedy nearest-neighbour minimum spanning tree
        List<int> connected = new List<int> { 0 };
        List<int> pending   = new List<int>();
        for (int i = 1; i < rooms.Count; i++)
            pending.Add(i);

        while (pending.Count > 0)
        {
            int bestFrom = -1, bestTo = -1;
            float bestDist = float.MaxValue;

            foreach (int c in connected)
            {
                Vector2Int cCenter = RoomCenter(rooms[c]);
                foreach (int p in pending)
                {
                    float d = Vector2Int.Distance(cCenter, RoomCenter(rooms[p]));
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestFrom = c;
                        bestTo   = p;
                    }
                }
            }

            if (bestFrom < 0)
                break;

            CarveCorridorBetween(RoomCenter(rooms[bestFrom]), RoomCenter(rooms[bestTo]));
            connected.Add(bestTo);
            pending.Remove(bestTo);
        }
    }

    private void CarveCorridorBetween(Vector2Int a, Vector2Int b)
    {
        if (Random.value > 0.5f)
        {
            CarveHorizontal(a.y, a.x, b.x);
            CarveVertical(b.x, a.y, b.y);
        }
        else
        {
            CarveVertical(a.x, a.y, b.y);
            CarveHorizontal(b.y, a.x, b.x);
        }
    }

    private void CarveHorizontal(int y, int x1, int x2)
    {
        int minX = Mathf.Min(x1, x2);
        int maxX = Mathf.Max(x1, x2);
        int half = corridorWidth / 2;
        for (int x = minX; x <= maxX; x++)
            for (int w = -half; w <= half; w++)
                SetFloor(x, y + w);
    }

    private void CarveVertical(int x, int y1, int y2)
    {
        int minY = Mathf.Min(y1, y2);
        int maxY = Mathf.Max(y1, y2);
        int half = corridorWidth / 2;
        for (int y = minY; y <= maxY; y++)
            for (int w = -half; w <= half; w++)
                SetFloor(x + w, y);
    }

    private void SetFloor(int x, int y)
    {
        if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
            floorMap[x, y] = true;
    }

    private void PaintTiles()
    {
        groundTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);

                if (floorMap[x, y])
                {
                    groundTilemap.SetTile(cell, floorTile);
                }
                else if (HasAdjacentFloor(x, y))
                {
                    wallTilemap.SetTile(cell, wallTile);
                }
            }
        }
    }

    private bool HasAdjacentFloor(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight && floorMap[nx, ny])
                    return true;
            }
        return false;
    }

    private void SetupWallCollision()
    {
        TilemapCollider2D col = wallTilemap.GetComponent<TilemapCollider2D>();
        if (col == null)
            col = wallTilemap.gameObject.AddComponent<TilemapCollider2D>();
        col.compositeOperation = Collider2D.CompositeOperation.Merge;

        Rigidbody2D rb = wallTilemap.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = wallTilemap.gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        CompositeCollider2D comp = wallTilemap.GetComponent<CompositeCollider2D>();
        if (comp == null)
            comp = wallTilemap.gameObject.AddComponent<CompositeCollider2D>();
        comp.geometryType = CompositeCollider2D.GeometryType.Polygons;
    }

    private void SpawnPlayer()
    {
        if (rooms.Count == 0)
            return;

        Vector3Int spawnCell = new Vector3Int(RoomCenter(rooms[0]).x, RoomCenter(rooms[0]).y, 0);
        PlayerSpawnWorldPosition = groundTilemap.GetCellCenterWorld(spawnCell);

        if (player != null)
            player.position = PlayerSpawnWorldPosition;
    }

    private static Vector2Int RoomCenter(RectInt room)
    {
        return new Vector2Int(room.xMin + room.width / 2, room.yMin + room.height / 2);
    }

    private void OnDrawGizmos()
    {
        if (rooms == null || groundTilemap == null)
            return;

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        foreach (RectInt room in rooms)
        {
            Vector3 center = groundTilemap.GetCellCenterWorld(
                new Vector3Int(room.xMin + room.width / 2, room.yMin + room.height / 2, 0));
            Gizmos.DrawWireCube(center, new Vector3(room.width, room.height, 0f));
        }
    }
}
