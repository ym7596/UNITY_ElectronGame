using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Core.GridSystem
{
    public interface IGridService
    {
        void Initialize();
        Vector2Int GridSize { get; set; }
        Vector2Int WorldToGrid(Vector3 worldPosition);
        Vector3 GridToWorld(Vector2Int gridPosition);
        bool IsWithinGrid(Vector2Int gridPosition);
            
        // --- 데이터 관리 기능 ---
        TileType GetTileType(Vector2Int gridPosition);
        void SetTileType(Vector2Int gridPosition, TileType type);
        bool IsOccupied(Vector2Int gridPosition);

        // --- 전선(Wire) 관리 기능 (그래프 기반으로 변경) ---
        void AddWireConnection(Vector2Int from, Vector2Int to);
        void RemoveWireNode(Vector2Int pos);
        Dictionary<Vector2Int, HashSet<Vector2Int>> GetWireGraph();
        void ClearWireConnections();
    }
}