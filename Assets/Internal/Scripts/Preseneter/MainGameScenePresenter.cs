using System;
using UnityEngine;
using VContainer.Unity;
using Internal.Scripts.Input;
using Internal.Scripts.Core.ElecSystem;

public class MainGameScenePresenter : IInitializable, IDisposable
{
    private readonly IInputService _inputService;
    private readonly IGridService _gridService;
    private readonly Camera _cam;

    public MainGameScenePresenter(IInputService inputService, IGridService gridService)
    {
        _inputService = inputService;
        _gridService = gridService;
        _cam = Camera.main;
    }

    public void Initialize()
    {
        _inputService.OnLeftClickPerformed += OnHandleLeftClick;
    }

    public void Dispose()
    {
        _inputService.OnLeftClickPerformed -= OnHandleLeftClick;
    }

    private void OnHandleLeftClick()
    {
        Ray ray = _cam.ScreenPointToRay(_inputService.MousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 2. 월드 좌표를 그리드 좌표로 변환 (인덱스)
            Vector2Int gridPos = _gridService.WorldToGrid(hit.point);
            
            if (_gridService.IsWithinGrid(gridPos))
            {
                // 3. 그리드 인덱스를 다시 타일의 정중앙 월드 좌표로 변환
                Vector3 snappedWorldPos = _gridService.GridToWorld(gridPos);
                
                // 4. 로그에 인덱스와 정교화된(Snapped) 월드 좌표를 모두 출력
                Debug.Log($"[GridClick] Index: {gridPos}, WorldPos: {snappedWorldPos}");
            }
        }
    }
}
