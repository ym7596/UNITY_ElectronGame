using System;
using UnityEngine;
using VContainer.Unity;

public class MainGameScenePresenter : IInitializable, IDisposable
{
    private readonly IInputService _inputService;
    public MainGameScenePresenter(IInputService inputService)
    {
        _inputService = inputService;
        Debug.Log("MainGameScenePresenter");
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
        Vector2 pos = _inputService.MousePosition;
        Debug.Log($"전선 생성 시도: {pos}");
    }
}
