using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Core.GridSystem
{
    public class GridVisualizer : MonoBehaviour
    {
        private IGridService gridService;
        
        [Header("Tile Prefabs")]
        [SerializeField] private GameObject groundPrefab;
        [SerializeField] private GameObject pathPrefab;
        [SerializeField] private GameObject waterPrefab;

        private Dictionary<Vector2Int, GameObject> _spawnedTiles = new Dictionary<Vector2Int, GameObject>();
        
        public void InitializeMap(IGridService gridService)
        {
            if (this.gridService == null)
                this.gridService = GetComponent<GridManager>();

            this.gridService = gridService;
          //  InitializeMap();
        }

        private void InitializeMap()
        {
            ClearMap();
            Debug.Log(gridService.GridSize);
            int halfW =  gridService.GridSize.x / 2;
            int halfH = gridService.GridSize.y / 2;

            for (int x = -halfW; x < halfW; x++)
            {
                for (int y = -halfH; y < halfH; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    TileType type = gridService.GetTileType(pos);
                    
                    // 기본적으로 비어있으면 Ground로 취급하거나, 
                    // 데이터가 설정된 대로 배치
                    SpawnTile(pos, type == TileType.Empty ? TileType.Ground : type);
                }
            }
        }

        private void SpawnTile(Vector2Int gridPos, TileType type)
        {
            GameObject prefab = GetPrefabForType(type);
            if (prefab == null) return;

            Vector3 worldPos = gridService.GridToWorld(gridPos);
            GameObject tile = Instantiate(prefab, worldPos, Quaternion.identity, transform);
            tile.name = $"Tile_{gridPos.x}_{gridPos.y}_{type}";
            
            _spawnedTiles[gridPos] = tile;
        }

        private GameObject GetPrefabForType(TileType type)
        {
            return type switch
            {
                TileType.Ground => groundPrefab,
                TileType.Path => pathPrefab,
                TileType.Water => waterPrefab,
                _ => groundPrefab // 기본값
            };
        }

        public void ClearMap()
        {
            foreach (var tile in _spawnedTiles.Values)
            {
                if (tile != null) Destroy(tile);
            }
            _spawnedTiles.Clear();
            
            // Transform 자식들도 정리 (혹시 모를 수동 배치 대비)
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
