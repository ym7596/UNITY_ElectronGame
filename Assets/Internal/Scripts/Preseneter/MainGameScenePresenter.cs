using System;
using UnityEngine;
using VContainer.Unity;
using Internal.Scripts.Core.GridSystem;
using Internal.Scripts.Core.TimeSystem;
using Internal.Scripts.Presenter;

public class MainGameScenePresenter : IInitializable, IDisposable, ITickable
{
    private readonly IInputService _inputService;
    private readonly IGridService _gridService;
    private readonly ITimeService _timeService;
    private readonly WirePlacer _wirePlacer;
    private readonly GameUIPresenter _uiPresenter;

    private readonly GridVisualizer _gridVisualizer;
    private readonly Camera _cam;

    private Vector3 _startClickPosition;
    private bool _isDragging;
    private bool _hasStartedPlacing; // 드래그 세션 당 한 번만 StartPlacing을 호출하기 위한 플래그

    public MainGameScenePresenter(
        IInputService inputService, 
        IGridService gridService,
        ITimeService timeService,
        WirePlacer wirePlacer,
        GridVisualizer gridVisualizer,
        GameUIPresenter uiPresenter)
    {
        Debug.Log("MainPresenter Initialized");
        _inputService = inputService;
        _gridService = gridService;
        _timeService = timeService;
        _wirePlacer = wirePlacer;
        _uiPresenter = uiPresenter;
        Debug.Log("MainPresenter Initialized2");
        _gridVisualizer = gridVisualizer;
        Debug.Log($"input service : {_inputService} grid service : {_gridService} time service : {_timeService}" +
                  $"wire placer : {_wirePlacer} ui presenter : {_uiPresenter} grid visualizer : {_gridVisualizer}");
        _cam = Camera.main;
    }

    public void Initialize()
    {
        Debug.Log($"GridService Initialized : {_gridService}");
        _gridService.Initialize();
        Debug.Log($"GridService Initialized : {_gridService}");
        _gridVisualizer.InitializeMap(_gridService);
        _wirePlacer.InitializeMap(_gridService);

        _inputService.OnLeftClickStarted += OnHandleLeftClickStarted;
        _inputService.OnLeftClickCanceled += OnHandleLeftClickCanceled;
        
        _inputService.OnRightClickPerformed += OnHandleRightClick;

        _uiPresenter.OnPartChanged += OnPartChanged;
        

        // 시간 이벤트 연결
        _timeService.OnDayPassed += OnDayPassed;
        _timeService.OnHouseSpawnRequested += OnHouseSpawn;
    }


    public void Dispose()
    {
        _inputService.OnLeftClickStarted -= OnHandleLeftClickStarted;
        _inputService.OnLeftClickCanceled -= OnHandleLeftClickCanceled;
        
        _inputService.OnRightClickPerformed -= OnHandleRightClick;

        _uiPresenter.OnPartChanged -= OnPartChanged;
        
        _timeService.OnDayPassed -= OnDayPassed;
        _timeService.OnHouseSpawnRequested -= OnHouseSpawn;
    }

    private void OnPartChanged(int index)
    {
        Debug.Log($"<color=green>MainPresenter: UI selected Part {index}. Updating Placement state...</color>");
        // 여기서 index에 따라 WirePlacer의 모드를 변경하거나, 
        // 다른 설치용 클래스(BuildingPlacer 등)에게 알림을 보낼 수 있습니다.
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
                if (!_hasStartedPlacing)
                {
                    _wirePlacer.StartPlacing(gridPos);
                    _hasStartedPlacing = true;
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

    private void OnDayPassed(int day)
    {
        Debug.Log($"<color=yellow>Presenter: UI Update - Day {day}</color>");
    }

    private void OnHouseSpawn()
    {
        Debug.Log("<color=cyan>Presenter: Signal - Time to Spawn a House!</color>");
    }

    private void OnHandleLeftClickStarted()
    {
        // 클릭 이벤트 시점의 좌표가 부정확할 수 있으므로 (Focus Gain 등), 
        // 실제 설치 시작 로직은 Tick의 첫 프레임으로 위임함.
        _isDragging = true;
        _hasStartedPlacing = false;
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
