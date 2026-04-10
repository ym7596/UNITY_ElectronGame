using System;
using UnityEngine;
using VContainer.Unity;
using Internal.Scripts.Core.GridSystem;
using Internal.Scripts.Core.TimeSystem;

public class MainGameScenePresenter : IInitializable, IDisposable, ITickable
{
    private readonly IInputService _inputService;
    private readonly IGridService _gridService;
    private readonly ITimeService _timeService;
    private readonly WirePlacer _wirePlacer;

    private readonly GridVisualizer _gridVisualizer;
    private readonly Camera _cam;

    private Vector3 _startClickPosition;
    private bool _isDragging;

    public MainGameScenePresenter(
        IInputService inputService, 
        IGridService gridService,
        ITimeService timeService,
        WirePlacer wirePlacer,
        GridVisualizer gridVisualizer)
    {
        _inputService = inputService;
        _gridService = gridService;
        _timeService = timeService;
        _wirePlacer = wirePlacer;

        _gridVisualizer = gridVisualizer;
        _cam = Camera.main;
    }

    public void Initialize()
    {
        _gridService.Initialize();
        _inputService.OnLeftClickStarted += OnHandleLeftClickStarted;
        _inputService.OnLeftClickCanceled += OnHandleLeftClickCanceled;
        
        _inputService.OnRightClickPerformed += OnHandleRightClick;
        
        _gridVisualizer.InitializeMap(_gridService);
        _wirePlacer.InitializeMap(_gridService);

        // 시간 이벤트 연결
        _timeService.OnDayPassed += (day) => Debug.Log($"<color=yellow>Presenter: UI Update - Day {day}</color>");
        _timeService.OnHouseSpawnRequested += () => Debug.Log("<color=cyan>Presenter: Signal - Time to Spawn a House!</color>");
    }


    public void Dispose()
    {
        _inputService.OnLeftClickStarted -= OnHandleLeftClickStarted;
        _inputService.OnLeftClickCanceled -= OnHandleLeftClickCanceled;
        
        _inputService.OnRightClickPerformed -= OnHandleRightClick;
    }

    public void Tick()
    {
        if (_isDragging)
        {
            Vector2 mousePos = _inputService.MousePosition;
            // 화면 밖으로 커서가 나갔을 때는 그리드 업데이트를 중단하여 잘못된 경로가 기록되는 것을 방지
            if (mousePos.x < 0 || mousePos.y < 0 || mousePos.x > Screen.width || mousePos.y > Screen.height)
            {
                return;
            }

            Ray ray = _cam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector2Int gridPos = _gridService.WorldToGrid(hit.point);
                
                // 아직 WirePlacer가 시작되지 않았다면 (첫 업데이트 프레임) 시작 처리
                if (!_wirePlacer.IsDragging)
                {
                    _wirePlacer.StartPlacing(gridPos);
                    _startClickPosition = hit.point;
                    TileType type = _gridService.GetTileType(gridPos);
                    Debug.Log($"[ClickStarted] WorldPos: {_startClickPosition}, GridPos: {gridPos}, TileType: {type}");
                }
                else
                {
                    _wirePlacer.UpdatePlacing(gridPos);
                }
            }
        }
    }

    private void OnHandleLeftClickStarted()
    {
        // 클릭 이벤트 시점의 좌표가 부정확할 수 있으므로 (Focus Gain 등), 
        // 실제 설치 시작 로직은 Tick의 첫 프레임으로 위임함.
        _isDragging = true;
    }

    private void OnHandleLeftClickCanceled()
    {
        _isDragging = false;
        _startClickPosition = Vector3.zero;
        _wirePlacer.StopPlacing();
        Debug.Log("[ClickCanceled] Position Reset and Placing Stopped");
    }

    private void OnHandleRightClick()
    {
        Vector2 mousePos = _inputService.MousePosition;
        if (mousePos.x < 0 || mousePos.y < 0 || mousePos.x > Screen.width || mousePos.y > Screen.height)
        {
            return;
        }

        Ray ray = _cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector2Int gridPos = _gridService.WorldToGrid(hit.point);
            TileType type = _gridService.GetTileType(gridPos);
            Debug.Log($"[RightClick] GridPos: {gridPos}, TileType: {type}");
            
            _wirePlacer.RemoveWireAt(gridPos);
        }
    }
}
