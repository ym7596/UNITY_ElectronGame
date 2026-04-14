using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Core.GridSystem
{
    public class WirePlacer : MonoBehaviour
    {
        private IGridService _gridManager;
        [SerializeField] private GameObject wirePrefab; // LineRenderer와 Wire 컴포넌트가 있는 프리팹

        [Header("Settings")]
        [SerializeField] private float yOffset = 0.2f;

        private Dictionary<(Vector2Int, Vector2Int), Wire> _edgeVisuals = new Dictionary<(Vector2Int, Vector2Int), Wire>();
        private List<Vector2Int> _currentPath = new List<Vector2Int>();
        private Wire _previewWire;
        private bool _isDragging = false;
        public bool IsDragging => _isDragging;

        public void InitializeMap(IGridService gridService)
        {
            _gridManager = gridService;
        }
        
        private void Start()
        {
            // 시작 시 모든 전선의 전력 상태 업데이트
            UpdateAllWiresPowerStatus();
        }

        public void StartPlacing(Vector2Int startPos)
        {
            if (!_gridManager.IsWithinGrid(startPos)) return;

            // 이미 진행 중인 프리뷰가 있다면 정리 (비정상 종료 대응)
            if (_previewWire != null) Destroy(_previewWire.gameObject);

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
            if (!_gridManager.IsWithinGrid(currentGridPos)) return;

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
                // 그래프에 모든 인접 연결 추가
                for (int i = 0; i < _currentPath.Count - 1; i++)
                {
                    _gridManager.AddWireConnection(_currentPath[i], _currentPath[i + 1]);
                }
                
                // 시각적 요소 동기화
                SyncAllVisuals();
                
                // 설치 직후 전력 상태 업데이트
                UpdateAllWiresPowerStatus();
            }

            if (_previewWire != null) Destroy(_previewWire.gameObject);
            _previewWire = null;
            _currentPath.Clear();
        }

        /// <summary>
        /// 특정 위치의 전선을 삭제합니다. (우클릭 시 호출용)
        /// </summary>
        public void RemoveWireAt(Vector2Int gridPos)
        {
            if (_gridManager.GetTileType(gridPos) != TileType.Wire) return;

            // 1. 그래프에서 노드 제거 (연결된 모든 에지 자동 제거됨)
            _gridManager.RemoveWireNode(gridPos);

            // 2. 시각적 요소 동기화
            SyncAllVisuals();

            // 3. 전력 상태 재계산
            UpdateAllWiresPowerStatus();
        }

        private void SyncAllVisuals()
        {
            var graph = _gridManager.GetWireGraph();
            HashSet<(Vector2Int, Vector2Int)> currentEdges = new HashSet<(Vector2Int, Vector2Int)>();

            // 모든 에지 수집
            foreach (var kvp in graph)
            {
                Vector2Int from = kvp.Key;
                foreach (var to in kvp.Value)
                {
                    var edge = GetEdgeKey(from, to);
                    currentEdges.Add(edge);
                }
            }

            // [삭제] 더 이상 존재하지 않는 에지 비주얼 제거
            List<(Vector2Int, Vector2Int)> edgesToRemove = new List<(Vector2Int, Vector2Int)>();
            foreach (var edge in _edgeVisuals.Keys)
            {
                if (!currentEdges.Contains(edge)) edgesToRemove.Add(edge);
            }
            foreach (var edge in edgesToRemove)
            {
                if (_edgeVisuals[edge] != null) Destroy(_edgeVisuals[edge].gameObject);
                _edgeVisuals.Remove(edge);
            }

            // [추가] 새로운 에지 비주얼 생성
            foreach (var edge in currentEdges)
            {
                if (!_edgeVisuals.ContainsKey(edge))
                {
                    GameObject go = Instantiate(wirePrefab, Vector3.zero, Quaternion.identity);
                    Wire wire = go.GetComponent<Wire>();
                    wire.name = $"Edge_{edge.Item1}_{edge.Item2}";
                    
                    List<Vector2Int> path = new List<Vector2Int> { edge.Item1, edge.Item2 };
                    wire.gridPositions = path;
                    UpdateWireVisual(wire);
                    
                    _edgeVisuals[edge] = wire;
                }
            }
        }

        private (Vector2Int, Vector2Int) GetEdgeKey(Vector2Int a, Vector2Int b)
        {
            if (a.x < b.x || (a.x == b.x && a.y < b.y)) return (a, b);
            return (b, a);
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
            var graph = _gridManager.GetWireGraph();
            // 모든 에지 상태 초기화
            foreach (var wire in _edgeVisuals.Values) wire.SetPowered(false);

            HashSet<Vector2Int> batteryTiles = new HashSet<Vector2Int>
            {
                new Vector2Int(0, 0), new Vector2Int(-1, 0),
                new Vector2Int(-1, -1), new Vector2Int(0, -1)
            };

            HashSet<Vector2Int> poweredNodes = new HashSet<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            // 1. 배터리에 닿아있는 노드들 시작점으로 설정
            foreach (var batteryPos in batteryTiles)
            {
                if (graph.ContainsKey(batteryPos))
                {
                    poweredNodes.Add(batteryPos);
                    queue.Enqueue(batteryPos);
                }
            }

            // 2. BFS 탐색
            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                if (graph.TryGetValue(current, out var neighbors))
                {
                    foreach (var next in neighbors)
                    {
                        if (!poweredNodes.Contains(next))
                        {
                            poweredNodes.Add(next);
                            queue.Enqueue(next);
                        }
                    }
                }
            }

            // 3. 비주얼 업데이트 (두 노드가 모두 전원이 들어온 에지만 유색으로?) 
            // 아니면 한쪽만 들어와도? 보통은 '연결된 모든 선'이므로 한쪽만 들어와도 전선 자체가 연결된 것이면 색이 변해야 함.
            // 여기서는 양 끝점 중 하나라도 전력 노드에 연결되어 있으면 powered 처리
            foreach (var kvp in _edgeVisuals)
            {
                var edge = kvp.Key;
                if (poweredNodes.Contains(edge.Item1) || poweredNodes.Contains(edge.Item2))
                {
                    kvp.Value.SetPowered(true);
                }
            }
            
            Debug.Log($"[PowerSystem] Updated. {poweredNodes.Count} nodes powered.");
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
            UpdateWireVisual(_previewWire, _currentPath);
        }

        private void UpdateWireVisual(Wire wire, List<Vector2Int> path = null)
        {
            if (wire == null) return;
            
            List<Vector2Int> positions = path ?? wire.gridPositions;
            if (positions == null || positions.Count == 0) return;

            List<Vector3> worldPoints = new List<Vector3>();
            foreach (var gridPos in positions)
            {
                Vector3 wp = _gridManager.GridToWorld(gridPos);
                wp.y = yOffset;
                worldPoints.Add(wp);
            }
            wire.SetPoints(worldPoints);
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
