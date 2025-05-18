using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GearDraggable : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 originalPosition;
    private GearTile gearTile;
    private Camera cam;
    private GearGridManager gridManager;

    void Start()
    {
        gearTile = GetComponent<GearTile>();
        cam = Camera.main;
        gridManager = FindObjectOfType<GearGridManager>();
    }

    void OnMouseDown()
    {
        if (gearTile.data.isPermanent) return;

        isDragging = true;
        originalPosition = transform.position;
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetPos = mouseWorldPos + offset;
        targetPos.z = 0;
        transform.position = targetPos;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;

        isDragging = false;

        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int snappedGridPos = Vector2Int.RoundToInt(new Vector2(mouseWorldPos.x, mouseWorldPos.y));

        if (gridManager.IsValidAndEmpty(snappedGridPos))
        {
            gridManager.MoveTile(gearTile, snappedGridPos);
        }
        else
        {
            // Snap back
            transform.position = originalPosition;
        }
    }
}