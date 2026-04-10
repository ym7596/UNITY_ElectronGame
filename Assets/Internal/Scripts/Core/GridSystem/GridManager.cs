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
        
        private void Start()
        {
            // 중앙 배터리 영역 초기화 (탐색의 출발점이 될 수 있도록)
            SetTileType(new Vector2Int(0, 0), TileType.Building);
            SetTileType(new Vector2Int(-1, 0), TileType.Building);
            SetTileType(new Vector2Int(-1, -1), TileType.Building);
            SetTileType(new Vector2Int(0, -1), TileType.Building);
            
            UpdateGridMaterial();
            SyncSceneTilemap(tileMap);
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
                    TileBase tile = sceneTilemap.GetTile(cellPos);

                    // 타일이 없으면 해당 칸을 Empty로 초기화 (이전에 채워졌던 데이터 청소)
                    if (tile == null)
                    {
                        SetTileType(gridPos, TileType.Empty);
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
                        // 그 외(땅 등)는 모두 Empty로 취급
                        SetTileType(gridPos, TileType.Empty);
                    }
                }
            }

            Debug.Log($"씬 타일 데이터 {count}개 동기화 완료! (Building 보호 로직 적용됨)");
        }
     
    }
}
