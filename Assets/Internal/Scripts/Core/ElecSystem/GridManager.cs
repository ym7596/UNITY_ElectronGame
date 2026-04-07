using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Core.ElecSystem
{
   
    public interface IGridService
    {
        Vector2Int WorldToGrid(Vector3 worldPosition);
        Vector3 GridToWorld(Vector2Int gridPosition);
        bool IsWithinGrid(Vector2Int gridPosition);
        
        // --- 데이터 관리 기능 추가 ---
        TileType GetTileType(Vector2Int gridPosition);
        void SetTileType(Vector2Int gridPosition, TileType type);
        bool IsOccupied(Vector2Int gridPosition);
    }

    public class GridManager : MonoBehaviour, IGridService
    {
        [Header("Settings")]
        public int Width = 30; // 30x30으로 상향
        public int Height = 30;
        public float CellSize = 2f; // 건물 사이즈에 맞춰 2.0으로 유지
        [Header("Visualization")]
        public bool ShowCoordinates = true;
        // 그리드 데이터를 관리하는 딕셔너리

        private Dictionary<Vector2Int, TileType> _gridData = new Dictionary<Vector2Int, TileType>();
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            // 중앙 정렬 기반: x/z 좌표를 셀 크기로 나누어 그리드 인덱스 산출
            return new Vector2Int(
                Mathf.FloorToInt(worldPosition.x / CellSize),
                Mathf.FloorToInt(worldPosition.z / CellSize)
            );
        }
        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            // 중앙 정렬 기반: 인덱스에 셀 크기를 곱하고 절반 오프셋을 더해 중심점 산출
            return new Vector3(
                gridPosition.x * CellSize + CellSize / 2f, 
                0, 
                gridPosition.y * CellSize + CellSize / 2f
            );
        }
        public bool IsWithinGrid(Vector2Int gridPosition)
        {
            int halfW = Width / 2;
            int halfH = Height / 2;
            return gridPosition.x >= -halfW && gridPosition.x < halfW &&
                   gridPosition.y >= -halfH && gridPosition.y < halfH;
        }
        // --- 데이터 관리 구현 ---
        public TileType GetTileType(Vector2Int gridPosition)
        {
            if (_gridData.TryGetValue(gridPosition, out var type))
                return type;
            return TileType.Empty;
        }
        
        public void SetTileType(Vector2Int gridPosition, TileType type)
        {
            if (!IsWithinGrid(gridPosition)) return;
            
            _gridData[gridPosition] = type;
            Debug.Log($"[GridData] ({gridPosition.x}, {gridPosition.y}) set to {type}");
        }
        
        public bool IsOccupied(Vector2Int gridPosition)
        {
            return GetTileType(gridPosition) != TileType.Empty;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.4f);
            float yOffset = 0.05f;
            
            int halfW = Width / 2;
            int halfH = Height / 2;
            for (int x = -halfW; x <= halfW; x++)
            {
                Gizmos.DrawLine(
                    new Vector3(x * CellSize, yOffset, -halfH * CellSize), 
                    new Vector3(x * CellSize, yOffset, halfH * CellSize)
                );
            }
            for (int z = -halfH; z <= halfH; z++)
            {
                Gizmos.DrawLine(
                    new Vector3(-halfW * CellSize, yOffset, z * CellSize), 
                    new Vector3(halfW * CellSize, yOffset, z * CellSize)
                );
            }
            if (ShowCoordinates)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.gray;
                style.fontSize = 7;
                style.alignment = TextAnchor.MiddleCenter;
                for (int x = -halfW; x < halfW; x++)
                {
                    for (int z = -halfH; z < halfH; z++)
                    {
                        Vector3 pos = GridToWorld(new Vector2Int(x, z));
                        pos.y = yOffset;
                        UnityEditor.Handles.Label(pos, $"({x},{z})", style);
                    }
                }
            }
        }
#endif
    }
}
