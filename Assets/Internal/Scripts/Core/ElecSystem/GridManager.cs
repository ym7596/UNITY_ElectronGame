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
        public Vector2Int PowerSourcePos = Vector2Int.zero; // 중심(배터리) 위치
        // 그리드 데이터를 관리하는 딕셔너리

        private Dictionary<Vector2Int, TileType> _gridData = new Dictionary<Vector2Int, TileType>();

        private void Start()
        {
            // 중앙 배터리 영역 초기화 (탐색의 출발점이 될 수 있도록)
            SetTileType(new Vector2Int(0, 0), TileType.Building);
            SetTileType(new Vector2Int(-1, 0), TileType.Building);
            SetTileType(new Vector2Int(-1, -1), TileType.Building);
            SetTileType(new Vector2Int(0, -1), TileType.Building);
        }

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

        /// <summary>
        /// 특정 위치가 배터리(전원)와 연결되어 있는지 BFS로 확인합니다.
        /// </summary>
        public bool IsConnectedToPower(Vector2Int targetPos)
        {
            Vector2Int[] powerSources = new Vector2Int[] 
            {
                new Vector2Int(0, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(-1, -1),
                new Vector2Int(0, -1)
            };

            foreach (var source in powerSources)
                if (targetPos == source) return true;

            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            foreach (var source in powerSources)
            {
                queue.Enqueue(source);
                visited.Add(source);
            }

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                if (current == targetPos) return true;

                // 상하좌우 대각선 인접 타일 검사
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        Vector2Int neighbor = current + new Vector2Int(x, y);

                        if (visited.Contains(neighbor)) continue;
                        
                        // 이웃이 전선이거나 건물인 경우에만 전기 흐름
                        TileType type = GetTileType(neighbor);
                        if (type == TileType.Wire || type == TileType.Building)
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
                
                if (visited.Count > 1000) break; // 안전장치
            }
            
            // Debug.Log($"[PowerCheck] {targetPos} fail. Path checked: {visited.Count}");
            return false;
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
