using UnityEngine;
using System;

public interface IInputService
{
    event Action OnLeftClickStarted;
    event Action OnLeftClickPerformed;
    event Action OnLeftClickCanceled;
    
    event Action OnRightClickPerformed;
    
    Vector2 MousePosition { get; }
    Vector2 MouseDelta { get; }
    bool IsPointerOverUI { get; }
}
