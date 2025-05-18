using System.Collections;
using UnityEngine;



public enum GearType
{
    Normal,
    Engine
}
[System.Serializable]
public class GearTileData
{
    public GearType type;
    public bool isPermanent;
    public Vector2Int gridPosition;

    public bool engineClockwise = true; // ← Add this
    public bool hasTopTooth;
    public bool hasBottomTooth;
    public bool hasLeftTooth;
    public bool hasRightTooth;

    public bool IsConnectedToDirection(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return hasTopTooth;
        if (dir == Vector2Int.down) return hasBottomTooth;
        if (dir == Vector2Int.left) return hasLeftTooth;
        if (dir == Vector2Int.right) return hasRightTooth;
        return false;
    }
}


public class Gears : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}