using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Internal.Scripts.Core.GridSystem
{
    public class GridManager : MonoBehaviour, IGridService
    {
        [Header("Settings")]
        public int Width = 30; // 30x30으로 상향
        public int Height = 30;
        public float CellSize = 2f; // 건물 사이즈에 맞춰 2.0으로 유지
        [Header("Visualization")]
        public Tilemap tileMap;
        public Renderer GridRenderer;
        public bool ShowInGameGrid = true;
        public bool ShowGizmos = false;
        public Vector2Int PowerSourcePos = Vector2Int.zero; // 중심(배터리) 위치
        // 그리드 데이터를 관리하는 딕셔너리
        
        public void Initialize()
        {
            GridSize = new Vector2Int(Width, Height);
            UpdateGridMaterial();
        }

        public Vector2Int GridSize { get; set; }
        private Dictionary<Vector2Int, TileType> _gridData = new Dictionary<Vector2Int, TileType>();
        private Dictionary<Vector2Int, HashSet<Vector2Int>> _wireGraph = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
        
        private void Start()
        {
            // 중앙 배터리 영역 초기화 (탐색의 출발점이 될 수 있도록)
            SetTileType(new Vector2Int(0, 0), TileType.Building);
            SetTileType(new Vector2Int(-1, 0), TileType.Building);
            SetTileType(new Vector2Int(-1, -1), TileType.Building);
            SetTileType(new Vector2Int(0, -1), TileType.Building);
            
            UpdateGridMaterial();
            SyncSceneTilemap(tileMap);
            
            // 씬에 이미 배치된 기존 전선들을 찾아 목록 복구
            // (그래프 방식에서는 이제 Wire 오브젝트들이 각자 자신의 gridPositions를 가지고 있으므로 이를 기반으로 그래프 빌드)
            RestoreExistingWires();
        }

        private void RestoreExistingWires()
        {
            _wireGraph.Clear();
            Wire[] existingWires = Object.FindObjectsByType<Wire>(FindObjectsSortMode.None);
            foreach (var w in existingWires)
            {
                if (w.gridPositions != null && w.gridPositions.Count > 1)
                {
                    for (int i = 0; i < w.gridPositions.Count - 1; i++)
                    {
                        AddWireConnection(w.gridPositions[i], w.gridPositions[i + 1]);
                    }
                }
            }
            Debug.Log($"[GridManager] Restored wire graph with {_wireGraph.Count} nodes.");
        }

        private void OnValidate()
        {
            UpdateGridMaterial();
        }

        public void UpdateGridMaterial()
        {
            if (GridRenderer == null) return;
            
            // 런타임이 아닐 때는 sharedMaterial 사용
            Material mat = Application.isPlaying ? GridRenderer.material : GridRenderer.sharedMaterial;
            if (mat == null || !mat.HasProperty("_CellSize")) return;

            mat.SetFloat("_CellSize", CellSize);
            mat.SetFloat("_GridHalfWidth", (Width / 2f) * CellSize);
            mat.SetFloat("_GridHalfHeight", (Height / 2f) * CellSize);
            
            if (GridRenderer.gameObject.activeSelf != ShowInGameGrid)
                GridRenderer.gameObject.SetActive(ShowInGameGrid);
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
            {
                // Empty는 초기 상태일 뿐이므로 Ground로 취급
                return type == TileType.Empty ? TileType.Ground : type;
            }
            return TileType.Ground;
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
        
        [ContextMenu("Sync Data Now")]
        public void SyncSceneTilemap(Tilemap sceneTilemap)
        {
            if (sceneTilemap == null)
            {
                Debug.LogError("타일맵이 연결되지 않았습니다!");
                return;
            }

            int halfW = Width / 2;
            int halfH = Height / 2;

            int count = 0;
            for (int x = -halfW; x < halfW; x++)
            {
                for (int y = -halfH; y < halfH; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    
                    // 핵심: 이미 빌딩이 설치된 칸은 타일맵 동기화에서 제외합니다.
                    if (GetTileType(gridPos) == TileType.Building) continue;

                    Vector3 worldPos = GridToWorld(gridPos);
                    Vector3Int cellPos = sceneTilemap.WorldToCell(worldPos);
                    TileBase tile = sceneTilemap.GetTile(cellPos); //참조타입이라 전역변수로 굳이 둘 필요 없음

                    // 타일이 없으면 해당 칸을 Ground로 초기화 (이전에 채워졌던 데이터 청소)
                    if (tile == null)
                    {
                        SetTileType(gridPos, TileType.Ground);
                        continue;
                    }

                    if (tile.name.Contains("Water"))
                    {
                        SetTileType(gridPos, TileType.Water);
                        count++;
                    }
                    else if (tile.name.Contains("Obstacle"))
                    {
                        SetTileType(gridPos, TileType.Obstacle);
                        count++;
                    }
                    else
                    {
                        // 그 외(땅 등)는 모두 Ground로 취급
                        SetTileType(gridPos, TileType.Ground);
                    }
                }
            }

            Debug.Log($"씬 타일 데이터 {count}개 동기화 완료! (Building 보호 로직 적용됨)");
        }
        
        // --- 전선(Wire) 관리 기능 구현 (그래프 기반) ---
        public Dictionary<Vector2Int, HashSet<Vector2Int>> GetWireGraph() => _wireGraph;

        public void AddWireConnection(Vector2Int from, Vector2Int to)
        {
            if (!_wireGraph.ContainsKey(from)) _wireGraph[from] = new HashSet<Vector2Int>();
            if (!_wireGraph.ContainsKey(to)) _wireGraph[to] = new HashSet<Vector2Int>();

            _wireGraph[from].Add(to);
            _wireGraph[to].Add(from);

            SetTileType(from, TileType.Wire);
            SetTileType(to, TileType.Wire);
        }

        public void RemoveWireNode(Vector2Int pos)
        {
            if (!_wireGraph.ContainsKey(pos)) return;

            // 연결된 이웃들로부터 이 노드 제거
            foreach (var neighbor in _wireGraph[pos])
            {
                if (_wireGraph.ContainsKey(neighbor))
                    _wireGraph[neighbor].Remove(pos);
            }

            _wireGraph.Remove(pos);
            SetTileType(pos, TileType.Ground);
        }

        public void ClearWireConnections()
        {
            _wireGraph.Clear();
        }
    }
}
