using UnityEngine;
using UnityEngine.Tilemaps;

// Attach to a parent GameObject that contains the tilemap layers for one height level.
// Automatically adjusts sorting order and wall collision when the player changes level.
public class HeightLevelGroup : MonoBehaviour
{
    [Header("Level")]
    [Tooltip("The height level this group represents (0 = ground, 1 = elevated, -1 = underground...).")]
    [SerializeField] private int level = 0;

    [Header("Tilemaps")]
    [SerializeField] private TilemapRenderer groundRenderer;
    [SerializeField] private TilemapRenderer wallsRenderer;
    [SerializeField] private TilemapRenderer propsRenderer;
    [SerializeField] private TilemapRenderer overheadRenderer;

    [Header("Base Sorting Orders (same level as player)")]
    [SerializeField] private int groundBaseOrder   = -10;
    [SerializeField] private int wallsBaseOrder    = -5;
    [SerializeField] private int propsBaseOrder    = 5;
    [SerializeField] private int overheadBaseOrder = 20;

    [Header("Order Offsets")]
    [Tooltip("Added to all sorting orders when the player is BELOW this level (tiles act as ceiling / bridge above player).")]
    [SerializeField] private int ceilingOffset = 200;
    [Tooltip("Added to all sorting orders when the player is ABOVE this level (tiles render far behind).")]
    [SerializeField] private int floorBelowOffset = -200;

    public void Initialize(int levelIndex, TilemapRenderer ground, TilemapRenderer walls, TilemapRenderer props, TilemapRenderer overhead)
    {
        level           = levelIndex;
        groundRenderer  = ground;
        wallsRenderer   = walls;
        propsRenderer   = props;
        overheadRenderer = overhead;
    }

    private void OnEnable()
    {
        PlayerLevel.AnyLevelChanged += OnPlayerLevelChanged;
    }

    private void OnDisable()
    {
        PlayerLevel.AnyLevelChanged -= OnPlayerLevelChanged;
    }

    private void Start()
    {
        PlayerLevel playerLevel = FindFirstObjectByType<PlayerLevel>();
        if (playerLevel != null)
            Apply(playerLevel.CurrentLevel);
    }

    private void OnPlayerLevelChanged(int _, int newLevel)
    {
        Apply(newLevel);
    }

    private void Apply(int playerLevel)
    {
        // delta > 0 → player above this group  (this group is floor below)
        // delta == 0 → player on this group     (normal)
        // delta < 0 → player below this group  (this group is ceiling above)
        int delta = playerLevel - level;
        int offset = delta == 0 ? 0 : (delta > 0 ? floorBelowOffset : ceilingOffset);

        SetOrder(groundRenderer,   groundBaseOrder   + offset);
        SetOrder(wallsRenderer,    wallsBaseOrder    + offset);
        SetOrder(propsRenderer,    propsBaseOrder    + offset);
        SetOrder(overheadRenderer, overheadBaseOrder + offset);

        SetWallsCollision(delta == 0);
    }

    private static void SetOrder(TilemapRenderer tilemapRenderer, int order)
    {
        if (tilemapRenderer != null)
            tilemapRenderer.sortingOrder = order;
    }

    private void SetWallsCollision(bool active)
    {
        if (wallsRenderer == null)
            return;

        CompositeCollider2D comp = wallsRenderer.GetComponent<CompositeCollider2D>();
        if (comp != null)
            comp.enabled = active;
    }
}
