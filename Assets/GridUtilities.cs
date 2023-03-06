using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridUtilities
{
    public const int TileSize = 1;
    public const float TileHalfSize = (float)TileSize / 2;
    public static Vector3 GetGridPosition(Vector3 worldPosition)
    {
        worldPosition.x = (int)(worldPosition.x / TileSize) * TileSize;
        worldPosition.y = (int)(worldPosition.y / TileSize) * TileSize;
        worldPosition.z = (int)(worldPosition.z / TileSize) * TileSize;
        return worldPosition;
    }
    public static Vector3 GetTileCenterFromWorld(Vector3 worldPosition)
    {
        worldPosition.x = (int)(worldPosition.x / TileSize) * TileSize + TileHalfSize;
        worldPosition.y = (int)(worldPosition.y / TileSize) * TileSize + TileHalfSize;
        worldPosition.z = (int)(worldPosition.z / TileSize) * TileSize + TileHalfSize;
        return worldPosition;
    }
    public static Vector3 GetTileCenterFromWorldXZ(Vector3 worldPosition)
    {
        worldPosition.x = Mathf.Floor(worldPosition.x / TileSize) * TileSize + TileHalfSize;
        worldPosition.z = Mathf.Floor(worldPosition.z / TileSize) * TileSize + TileHalfSize;
        return worldPosition;
    }
    public static void SnapToTileCenter(this Vector3 worldPosition)
    {
        worldPosition = GetTileCenterFromWorld(worldPosition);
    }
}
