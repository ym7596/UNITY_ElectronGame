using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
        public bool IsPointerOverUI { get; private set; }

        private void Awake()
        {
            _dduRInput = new DduRInput();
            _dduRInput.Enable();

            _dduRInput.dduRActionMap.Click.started += _ => 
            {
                IsPointerOverUI = CheckIfPointerOverUI();
                if (!IsPointerOverUI) OnLeftClickStarted?.Invoke();
            };
            
            _dduRInput.dduRActionMap.Click.performed += _ => 
            {
                if (!IsPointerOverUI) OnLeftClickPerformed?.Invoke();
            };
            
            _dduRInput.dduRActionMap.Click.canceled += _ => 
            {
                if (!IsPointerOverUI) OnLeftClickCanceled?.Invoke();
            };

            _dduRInput.dduRActionMap.RightClick.started += _ => 
            {
                IsPointerOverUI = CheckIfPointerOverUI();
                if (!IsPointerOverUI) OnRightClickStarted?.Invoke();
            };
            
            _dduRInput.dduRActionMap.RightClick.performed += _ => 
            {
                if (!IsPointerOverUI) OnRightClickPerformed?.Invoke();
            };
        }

        private bool CheckIfPointerOverUI()
        {
            if (EventSystem.current == null) return false;
            
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = MousePosition
            };
            
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                OnLeftClickCanceled?.Invoke();
            }
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
