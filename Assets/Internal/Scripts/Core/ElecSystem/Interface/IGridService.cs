using UnityEngine;

public interface IGridService
{
    void Initialize();
    Vector2Int GridSize { get; set; }
    Vector2Int WorldToGrid(Vector3 worldPosition);
    Vector3 GridToWorld(Vector2Int gridPosition);
    bool IsWithinGrid(Vector2Int gridPosition);
        
    // --- 데이터 관리 기능 추가 ---
    TileType GetTileType(Vector2Int gridPosition);
    void SetTileType(Vector2Int gridPosition, TileType type);
    bool IsOccupied(Vector2Int gridPosition);
}