using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, IInputService
{
    private DduRInput _dduRInput;
    public event Action OnLeftClickStarted;
    public event Action OnLeftClickPerformed;
    public event Action OnLeftClickCanceled;
    public event Action OnRightClickPerformed;

    public Vector2 MousePosition => _dduRInput.dduRActionMap.Position.ReadValue<Vector2>();
    public Vector2 MouseDelta => _dduRInput.dduRActionMap.Delta.ReadValue<Vector2>();
    
    private void Awake()
    {
        _dduRInput = new DduRInput();
        _dduRInput.Enable();
        // 입력 액션 연결
        _dduRInput.dduRActionMap.Click.started += _ => OnLeftClickStarted?.Invoke();
        _dduRInput.dduRActionMap.Click.performed += _ => OnLeftClickPerformed?.Invoke();
        _dduRInput.dduRActionMap.Click.canceled += _ => OnLeftClickCanceled?.Invoke();
        
        _dduRInput.dduRActionMap.RightClick.performed += _ => OnRightClickPerformed?.Invoke();
    }
    private void OnDestroy() => _dduRInput?.Dispose();


}
