using System.Collections.Generic;
using UnityEngine;

public class GearGridManager : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public GameObject gearPrefab;
    public GameObject placeholderPrefab;

    private Dictionary<Vector2Int, GearTile> grid = new();
    private Dictionary<Vector2Int, GameObject> placeholderGrid = new();

    [Header("Initial Gear Config")]
    public List<GearTileData> initialTiles;

    private float tickInterval = 1f;
    private float tickTimer = 0f;

    void Start()
    {
        BuildGrid();
    }

    public bool IsValidAndEmpty(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            return false;

        return !grid.ContainsKey(pos);
    }

    public void MoveTile(GearTile tile, Vector2Int newPos)
    {
        Vector2Int oldPos = tile.GridPosition;
        if (newPos == oldPos) return;

        // Remove gear from old position
        if (grid.ContainsKey(oldPos))
        {
            grid.Remove(oldPos);
        }

        // Destroy placeholder at new position
        if (placeholderGrid.TryGetValue(newPos, out GameObject newPosPlaceholder))
        {
            Destroy(newPosPlaceholder);
            placeholderGrid.Remove(newPos);
        }

        // Create a placeholder at old position
        Vector3 oldWorldPos = new Vector3(oldPos.x, oldPos.y, 0);
        GameObject oldPlaceholder = Instantiate(placeholderPrefab, oldWorldPos, Quaternion.identity, transform);
        placeholderGrid[oldPos] = oldPlaceholder;

        // Add gear to new position
        grid[newPos] = tile;
        tile.data.gridPosition = newPos;
        tile.transform.position = new Vector3(newPos.x, newPos.y, 0);

        Debug.Log($"[Grid] Moved gear from {oldPos} to {newPos}");
    }

    void BuildGrid()
    {
        Dictionary<Vector2Int, GearTileData> initialGearLookup = new();
        foreach (var gearData in initialTiles)
        {
            initialGearLookup[gearData.gridPosition] = gearData;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Vector3 worldPos = new Vector3(x, y, 0);

                if (initialGearLookup.TryGetValue(pos, out GearTileData gearData))
                {
                    // Instantiate actual gear
                    GameObject gearGO = Instantiate(gearPrefab, worldPos, Quaternion.identity, transform);
                    GearTile gearTile = gearGO.GetComponent<GearTile>();
                    gearTile.Initialize(gearData);
                    grid[pos] = gearTile;
                }
                else
                {
                    // Instantiate placeholder
                    GameObject placeholder = Instantiate(placeholderPrefab, worldPos, Quaternion.identity, transform);
                    placeholderGrid[pos] = placeholder;
                }
            }
        }
    }

    void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;

            foreach (var gear in grid.Values)
            {
                if (gear.data.type == GearType.Engine)
                {
                    gear.Tick(this);
                }
            }
        }
    }

    public GearTile GetTile(Vector2Int pos)
    {
        return grid.TryGetValue(pos, out var tile) ? tile : null;
    }
}
