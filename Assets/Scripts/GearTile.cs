using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GearTile : MonoBehaviour
{
    public GearTileData data;
    public Vector2Int GridPosition => data.gridPosition;

    private int currentRotationIndex = 0;
    private int previousRotationIndex = 0;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationDuration = 0.5f;
    [SerializeField] private float chainDelayFactor = 0.15f; // Delay for chained rotations
    [SerializeField] private Ease rotationEase = Ease.InOutQuad;
    
    [Header("Push Effect")]
    [SerializeField] private float pushEffectDistance = 0.05f;
    [SerializeField] private float pushEffectDuration = 0.2f;

    [Header("Visual References")]
    public SpriteRenderer baseRenderer;
    public Sprite fullSprite;

    public GameObject toothUp;
    public GameObject toothDown;
    public GameObject toothLeft;
    public GameObject toothRight;
    
    private bool isRotating = false;
    private Sequence rotationSequence;
    
    // Added method to calculate initial rotation offset
    private float GetInitialRotationOffset()
    {
        // Simple checkerboard pattern to alternate gear orientations
        bool shouldOffset = (data.gridPosition.x + data.gridPosition.y) % 2 == 1;
        
        // Rotate by 22.5 degrees (half a tooth) for proper meshing
        return shouldOffset ? 22.5f : 0f;
    }

    public void Initialize(GearTileData tileData)
    {
        data = tileData;
        transform.localRotation = Quaternion.identity;
        currentRotationIndex = 0;
        previousRotationIndex = 0;
        
        // Apply initial rotation offset based on grid position
        float initialOffset = GetInitialRotationOffset();
        if (initialOffset != 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, initialOffset);
        }

        if (tileData.type == GearType.Normal)
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
        if (data.type != GearType.Engine || isRotating) return;

        Debug.Log($"[Tick] Engine at {data.gridPosition} ticking ({(data.engineClockwise ? "CW" : "CCW")})");

        // Use original logic with visual enhancements
        Rotate(data.engineClockwise);

        HashSet<GearTile> visited = new() { this };
        PropagateRotation(gridManager, data.engineClockwise, visited);
    }
    
    public void Rotate(bool clockwise)
    {
        if (isRotating) return;
        isRotating = true;
        
        previousRotationIndex = currentRotationIndex;

        int dir = clockwise ? 1 : -1;
        currentRotationIndex = (currentRotationIndex + dir + 4) % 4;
    
        
        float angle = clockwise ? -45f : 45f;
        angle=data.type == GearType.Normal ? angle : angle * 2;
        float targetAngle = transform.eulerAngles.z + angle;
        
        // Enhanced rotation with easing for a more mechanical feel
        transform.DORotate(new Vector3(0, 0, targetAngle), rotationDuration)
            .SetEase(rotationEase)
            .OnComplete(() => isRotating = false);
    }
    
    private void PropagateRotation(GearGridManager gridManager, bool clockwise, HashSet<GearTile> visited)
    {
        foreach (var dir in DirectionUtils.CardinalDirections)
        {
            // We should ONLY check wasConnected - a gear can only push another
            // if they were connected BEFORE the rotation
            bool wasConnected = HadToothInWorldDir(dir);
        
            if (!wasConnected) continue;

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
            // Similarly, only check if the neighbor was connected back
            bool neighborWasConnected = neighbor.HadToothInWorldDir(opposite);

            if (!neighborWasConnected)
            {
                Debug.Log($"[Propagate] Neighbor at {neighborPos} not connected back in direction {opposite}");
                continue;
            }

            Debug.Log($"[Propagate] Rotating neighbor at {neighborPos} ← {(clockwise ? "CCW" : "CW")}");
        
            // Add visual push effect before rotating
            ApplyPushEffect(neighbor, dir);
        
            // Add to visited before propagating further
            visited.Add(neighbor);
        
            // Use a slight delay for chain reaction feel
            DOVirtual.DelayedCall(chainDelayFactor, () => {
                neighbor.Rotate(!clockwise);
                neighbor.PropagateRotation(gridManager, !clockwise, visited);
            });
        }
    }
    private void ApplyPushEffect(GearTile targetGear, Vector2Int direction)
    {
        // Calculate push direction vector
        Vector2 pushDir = new Vector2(direction.x, direction.y).normalized;
        
        // Visual push effect - slight move in push direction then back
        Vector3 originalPos = targetGear.transform.position;
        Vector3 pushPos = originalPos + new Vector3(pushDir.x * pushEffectDistance, pushDir.y * pushEffectDistance);
        
        // Create a sequence for the push effect
        Sequence pushSequence = DOTween.Sequence();
        
        // Push outward
        pushSequence.Append(targetGear.transform.DOMove(pushPos, pushEffectDuration * 0.5f)
            .SetEase(Ease.OutQuad));
        
        // Return to original position
        pushSequence.Append(targetGear.transform.DOMove(originalPos, pushEffectDuration * 0.5f)
            .SetEase(Ease.InQuad));
        
        pushSequence.Play();
    }

    public bool HasToothInWorldDir(Vector2Int worldDir)
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

    public bool HadToothInWorldDir(Vector2Int worldDir)
    {
        int baseDirIndex = DirToIndex(worldDir);
        int rotatedIndex = (baseDirIndex - previousRotationIndex + 4) % 4;

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
}