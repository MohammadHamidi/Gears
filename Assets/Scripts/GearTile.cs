using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine;

public class GearTile : MonoBehaviour
{
    public GearTileData data;
    public Vector2Int GridPosition => data.gridPosition;

    private int currentRotationIndex = 0; // 0 = 0°, 1 = 90°, 2 = 180°, 3 = 270°

    [Header("Visual References")]
    public SpriteRenderer baseRenderer;
    public Sprite fullSprite;

    public GameObject toothUp;
    public GameObject toothDown;
    public GameObject toothLeft;
    public GameObject toothRight;

    public void Initialize(GearTileData tileData)
    {
        data = tileData;
        transform.localRotation = Quaternion.identity;
        currentRotationIndex = 0;

        
        if (tileData.type==GearType.Normal)
        {
            baseRenderer.sprite = fullSprite;
            toothUp.SetActive(false);
            toothRight.SetActive(false);
            toothDown.SetActive(false);
            toothLeft.SetActive(false);
            return;
        }
        toothUp.SetActive(data.hasTopTooth);
        toothRight.SetActive(data.hasRightTooth);
        toothDown.SetActive(data.hasBottomTooth);
        toothLeft.SetActive(data.hasLeftTooth);
        
    }

    public void Tick(GearGridManager gridManager)
    {
        if (data.type != GearType.Engine) return;

        Debug.Log($"[Tick] Engine at {data.gridPosition} ticking ({(data.engineClockwise ? "CW" : "CCW")})");

        Rotate(data.engineClockwise);

        HashSet<GearTile> visited = new() { this };
        PropagateRotation(gridManager, data.engineClockwise, visited);
    }

    public void Rotate(bool clockwise)
    {
        int dir = clockwise ? 1 : -1;
        currentRotationIndex = (currentRotationIndex + dir + 4) % 4;

        float angle = clockwise ? -90f : 90f;
        transform.DORotate(transform.eulerAngles + new Vector3(0, 0, angle), 0.3f);
    }

    public bool IsConnectedToDirection(Vector2Int worldDir)
    {
        int baseDirIndex = DirToIndex(worldDir);
        int rotatedIndex = (baseDirIndex - currentRotationIndex + 4) % 4;

        return rotatedIndex switch
        {
            0 => data.hasTopTooth,
            1 => data.hasRightTooth,
            2 => data.hasBottomTooth,
            3 => data.hasLeftTooth,
            _ => false
        };
    }

    private int DirToIndex(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return 0;
        if (dir == Vector2Int.right) return 1;
        if (dir == Vector2Int.down) return 2;
        if (dir == Vector2Int.left) return 3;
        return -1;
    }

    private void PropagateRotation(GearGridManager gridManager, bool clockwise, HashSet<GearTile> visited)
    {
        foreach (var dir in DirectionUtils.CardinalDirections)
        {
            if (!IsConnectedToDirection(dir)) continue;

            Vector2Int neighborPos = data.gridPosition + dir;
            GearTile neighbor = gridManager.GetTile(neighborPos);

            if (neighbor == null)
            {
                Debug.Log($"[Propagate] No neighbor at {neighborPos}");
                continue;
            }

            if (visited.Contains(neighbor))
            {
                Debug.Log($"[Propagate] Already visited {neighborPos}");
                continue;
            }

            Vector2Int opposite = DirectionUtils.Opposite(dir);
            if (!neighbor.data.IsConnectedToDirection(opposite))
            {
                Debug.Log($"[Propagate] Neighbor at {neighborPos} not connected back in direction {opposite}");
                continue;
            }

            Debug.Log($"[Propagate] Rotating neighbor at {neighborPos} ← {(clockwise ? "CCW" : "CW")}");
            visited.Add(neighbor);
            neighbor.Rotate(!clockwise);
            neighbor.PropagateRotation(gridManager, !clockwise, visited);
        }
    }
}
