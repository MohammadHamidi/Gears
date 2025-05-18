using System.Collections.Generic;
using UnityEngine;

public class GearGridManager : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public GameObject gearPrefab;
    public GameObject placeholderPrefab;

    private Dictionary<Vector2Int, GearTile> grid = new();

    [Header("Initial Gear Config")]
    public List<GearTileData> initialTiles;

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
        if (grid.ContainsKey(tile.GridPosition))
            grid.Remove(tile.GridPosition);

        tile.data.gridPosition = newPos;
        grid[newPos] = tile;

        tile.transform.position = new Vector3(newPos.x, newPos.y, 0);
    }

    void BuildGrid()
    {
        // Build a lookup for initial gear positions
        Dictionary<Vector2Int, GearTileData> initialGearLookup = new();
        foreach (var gearData in initialTiles)
        {
            initialGearLookup[gearData.gridPosition] = gearData;
        }

        // Loop through entire grid space
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
                    // Instantiate empty placeholder
                    Instantiate(placeholderPrefab, worldPos, Quaternion.identity, transform);
                }
            }
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Tick all engine gears for testing
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