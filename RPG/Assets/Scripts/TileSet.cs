using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileSet", menuName = "RPG/TileSet")]
public class TileSet : ScriptableObject
{
    [Tooltip("All tiles in this set. Add duplicates to increase probability.")]
    public TileBase[] tiles;

    public bool IsEmpty => tiles == null || tiles.Length == 0;

    public TileBase PickRandom()
    {
        if (IsEmpty)
            return null;

        return tiles[Random.Range(0, tiles.Length)];
    }
}
