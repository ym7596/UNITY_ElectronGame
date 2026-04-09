using System;
using UnityEngine;
using VContainer.Unity;
using Internal.Scripts.Input;
using Internal.Scripts.Core.ElecSystem;

public class MainGameScenePresenter : IInitializable, IDisposable, ITickable
{
    private readonly IInputService _inputService;
    private readonly IGridService _gridService;
    private readonly WirePlacer _wirePlacer;
    private readonly Camera _cam;

    private Vector3 _startClickPosition;
    private bool _isDragging;

    public MainGameScenePresenter(
        IInputService inputService, 
        IGridService gridService,
        WirePlacer wirePlacer)
    {
        _inputService = inputService;
        _gridService = gridService;
        _wirePlacer = wirePlacer;
        _cam = Camera.main;
    }

    public void Initialize()
    {
        _inputService.OnLeftClickStarted += OnHandleLeftClickStarted;
        _inputService.OnLeftClickCanceled += OnHandleLeftClickCanceled;
        
        _inputService.OnRightClickPerformed += OnHandleRightClick;
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
            Ray ray = _cam.ScreenPointToRay(_inputService.MousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector2Int gridPos = _gridService.WorldToGrid(hit.point);
                _wirePlacer.UpdatePlacing(gridPos);
            }
        }
    }

    private void OnHandleLeftClickStarted()
    {
        Ray ray = _cam.ScreenPointToRay(_inputService.MousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            _startClickPosition = hit.point;
            Vector2Int gridPos = _gridService.WorldToGrid(hit.point);
            _isDragging = true;
            _wirePlacer.StartPlacing(gridPos);
            
            Debug.Log($"[ClickStarted] WorldPos: {_startClickPosition}, GridPos: {gridPos}");
        }
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
        Ray ray = _cam.ScreenPointToRay(_inputService.MousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector2Int gridPos = _gridService.WorldToGrid(hit.point);
            _wirePlacer.RemoveWireAt(gridPos);
            Debug.Log($"[RightClick] Removed wire at {gridPos}");
        }
    }
}
