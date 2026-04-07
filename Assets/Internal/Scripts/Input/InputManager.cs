using System;
using UnityEngine;

namespace Internal.Scripts.Input
{
    public class InputManager : MonoBehaviour, IInputService
    {
        private DduRInput _dduRInput;

        public event Action OnLeftClickStarted;
        public event Action OnLeftClickPerformed;
        public event Action OnLeftClickCanceled;
        public event Action OnRightClickStarted;
        public event Action OnRightClickPerformed;

        public Vector2 MousePosition => _dduRInput.dduRActionMap.Position.ReadValue<Vector2>();
        public Vector2 MouseDelta => _dduRInput.dduRActionMap.Delta.ReadValue<Vector2>();

        private void Awake()
        {
            _dduRInput = new DduRInput();
            _dduRInput.Enable();

            _dduRInput.dduRActionMap.Click.started += _ => OnLeftClickStarted?.Invoke();
            _dduRInput.dduRActionMap.Click.performed += _ => OnLeftClickPerformed?.Invoke();
            _dduRInput.dduRActionMap.Click.canceled += _ => OnLeftClickCanceled?.Invoke();

            _dduRInput.dduRActionMap.RightClick.started += _ => OnRightClickStarted?.Invoke();
            _dduRInput.dduRActionMap.RightClick.performed += _ => OnRightClickPerformed?.Invoke();
        }

        private void OnDestroy()
        {
            if (_dduRInput != null)
            {
                _dduRInput.Disable();
                _dduRInput.Dispose();
            }
        }
    }
}
