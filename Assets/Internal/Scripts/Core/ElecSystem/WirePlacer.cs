using System.Collections.Generic;
using UnityEngine;
using Internal.Scripts.Input;

namespace Internal.Scripts.Core.ElecSystem
{
    public class WirePlacer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private GameObject wirePrefab; // LineRenderer와 Wire 컴포넌트가 있는 프리팹

        [Header("Settings")]
        [SerializeField] private float yOffset = 0.2f;

        private List<Vector2Int> _currentPath = new List<Vector2Int>();
        private List<(List<Vector2Int> path, Wire wire)> _allWires = new List<(List<Vector2Int>, Wire)>();
        private Wire _previewWire;
        private bool _isDragging = false;

        private void Start()
        {
            if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();

            // 씬에 이미 배치된 기존 전선들을 찾아 목록 복구
            RestoreExistingWires();
            
            // 시작 시 모든 전선의 전력 상태 업데이트
            UpdateAllWiresPowerStatus();
        }

        private void RestoreExistingWires()
        {
            _allWires.Clear();
            Wire[] existingWires = Object.FindObjectsByType<Wire>(FindObjectsSortMode.None);
            foreach (var w in existingWires)
            {
                if (w.gridPositions != null && w.gridPositions.Count > 0)
                {
                    _allWires.Add((new List<Vector2Int>(w.gridPositions), w));
                }
            }
            Debug.Log($"[WirePlacer] Restored {_allWires.Count} existing wires from scene.");
        }

        public void StartPlacing(Vector2Int startPos)
        {
            if (!gridManager.IsWithinGrid(startPos)) return;

            _isDragging = true;
            _currentPath.Clear();
            _currentPath.Add(startPos);

            // 프리뷰용 와이어 생성
            if (wirePrefab != null)
            {
                GameObject go = Instantiate(wirePrefab, Vector3.zero, Quaternion.identity);
                _previewWire = go.GetComponent<Wire>();
                UpdatePreview();
            }
        }

        public void UpdatePlacing(Vector2Int currentGridPos)
        {
            if (!_isDragging) return;
            if (!gridManager.IsWithinGrid(currentGridPos)) return;

            Vector2Int lastPos = _currentPath[_currentPath.Count - 1];

            if (currentGridPos != lastPos)
            {
                // 바로 이전 칸으로 되돌아가는 경우 (Undo 효과)
                if (_currentPath.Count > 1 && currentGridPos == _currentPath[_currentPath.Count - 2])
                {
                    _currentPath.RemoveAt(_currentPath.Count - 1);
                    UpdatePreview();
                }
                // 새로운 칸으로 이동하는 경우 (인접한 경우만 추가)
                else if (IsAdjacent(lastPos, currentGridPos))
                {
                    if (!_currentPath.Contains(currentGridPos))
                    {
                        _currentPath.Add(currentGridPos);
                        UpdatePreview();
                    }
                }
                // 멀리 떨어진 경우 (미니 모토웨이처럼 사이를 채워줌)
                else
                {
                    FillPathTo(currentGridPos);
                    UpdatePreview();
                }
            }
        }

        public void StopPlacing()
        {
            if (!_isDragging) return;
            _isDragging = false;

            if (_currentPath.Count > 1)
            {
                // 실제 그리드 데이터에 전선 정보 기록
                foreach (var pos in _currentPath)
                {
                    gridManager.SetTileType(pos, TileType.Wire);
                }
                
                _previewWire.name = "Wire_Final";
                _previewWire.gridPositions = new List<Vector2Int>(_currentPath); // 위치 정보 저장
                _allWires.Add((new List<Vector2Int>(_currentPath), _previewWire));
                
                // 설치 직후 전력 상태 업데이트
                UpdateAllWiresPowerStatus();
            }
            else
            {
                if (_previewWire != null) Destroy(_previewWire.gameObject);
            }

            _previewWire = null;
            _currentPath.Clear();
        }

        /// <summary>
        /// 특정 위치의 전선을 삭제합니다. (우클릭 시 호출용)
        /// </summary>
        public void RemoveWireAt(Vector2Int gridPos)
        {
            if (gridManager.GetTileType(gridPos) != TileType.Wire) return;

            gridManager.SetTileType(gridPos, TileType.Empty);

            // 해당 좌표를 포함하는 모든 전선 오브젝트 찾기
            for (int i = _allWires.Count - 1; i >= 0; i--)
            {
                if (_allWires[i].path.Contains(gridPos))
                {
                    // 미니 모토웨이처럼 전체 전선을 지울지, 해당 좌표만 뺄지 결정 가능
                    // 여기서는 일단 해당 전선 덩어리 전체를 삭제하는 방식으로 구현
                    Destroy(_allWires[i].wire.gameObject);
                    
                    // 그리드 데이터에서도 해당 덩어리의 나머지 좌표들 삭제
                    foreach (var p in _allWires[i].path)
                    {
                        if (gridManager.GetTileType(p) == TileType.Wire)
                            gridManager.SetTileType(p, TileType.Empty);
                    }
                    
                    _allWires.RemoveAt(i);
                }
            }

            // 삭제 후 전력 상태 재계산
            UpdateAllWiresPowerStatus();
        }

        /// <summary>
        /// 설치된 모든 전선의 전력 공급 상태를 확인하고 시각 효과를 업데이트합니다.
        /// </summary>
        /// <summary>
        /// 설치된 모든 전선의 전력 공급 상태를 확인하고 시각 효과를 업데이트합니다.
        /// 노드(좌표 공유) 기반으로 연결성을 판단합니다.
        /// </summary>
        public void UpdateAllWiresPowerStatus()
        {
            // 모든 전선의 상태 초기화
            foreach (var item in _allWires) item.wire.SetPowered(false);

            HashSet<Vector2Int> batteryTiles = new HashSet<Vector2Int>
            {
                new Vector2Int(0, 0), new Vector2Int(-1, 0),
                new Vector2Int(-1, -1), new Vector2Int(0, -1)
            };

            HashSet<int> poweredWireIndices = new HashSet<int>();
            Queue<int> checkQueue = new Queue<int>();

            // 1. 배터리에 직접 닿은 전선들 먼저 찾기
            for (int i = 0; i < _allWires.Count; i++)
            {
                foreach (var pos in _allWires[i].path)
                {
                    if (batteryTiles.Contains(pos))
                    {
                        poweredWireIndices.Add(i);
                        checkQueue.Enqueue(i);
                        break;
                    }
                }
            }

            // 2. BFS로 연결된 전선들 추적 (공유하는 좌표가 있는지 확인)
            while (checkQueue.Count > 0)
            {
                int currentIdx = checkQueue.Dequeue();
                var currentWirePath = _allWires[currentIdx].path;

                for (int nextIdx = 0; nextIdx < _allWires.Count; nextIdx++)
                {
                    if (poweredWireIndices.Contains(nextIdx)) continue;

                    // 두 전선이 겹치는 좌표(노드)가 있는지 확인
                    if (HasSharedPosition(currentWirePath, _allWires[nextIdx].path))
                    {
                        poweredWireIndices.Add(nextIdx);
                        checkQueue.Enqueue(nextIdx);
                    }
                }
            }

            // 3. 결과 적용
            foreach (int idx in poweredWireIndices)
            {
                _allWires[idx].wire.SetPowered(true);
            }
            
            Debug.Log($"[PowerSystem] Updated. {poweredWireIndices.Count} wires connected via nodes.");
        }

        private bool HasSharedPosition(List<Vector2Int> pathA, List<Vector2Int> pathB)
        {
            // 한쪽 경로를 HashSet에 넣어 비교 속도 향상
            HashSet<Vector2Int> setA = new HashSet<Vector2Int>(pathA);
            foreach (var pos in pathB)
            {
                if (setA.Contains(pos)) return true;
            }
            return false;
        }

        private void UpdatePreview()
        {
            if (_previewWire == null) return;

            List<Vector3> worldPoints = new List<Vector3>();
            foreach (var gridPos in _currentPath)
            {
                Vector3 wp = gridManager.GridToWorld(gridPos);
                wp.y = yOffset;
                worldPoints.Add(wp);
            }
            _previewWire.SetPoints(worldPoints);
        }

        private bool IsAdjacent(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) <= 1 && Mathf.Abs(a.y - b.y) <= 1;
        }

        private void FillPathTo(Vector2Int target)
        {
            // 간단한 직선 채우기 로직 (브레젠험 알고리즘 변형)
            Vector2Int current = _currentPath[_currentPath.Count - 1];
            
            while (current != target)
            {
                int stepX = Mathf.Clamp(target.x - current.x, -1, 1);
                int stepY = Mathf.Clamp(target.y - current.y, -1, 1);
                
                current += new Vector2Int(stepX, stepY);
                
                if (!_currentPath.Contains(current))
                {
                    _currentPath.Add(current);
                }
                
                if (_currentPath.Count > 100) break; // 무한루프 방지
            }
        }
    }
}
