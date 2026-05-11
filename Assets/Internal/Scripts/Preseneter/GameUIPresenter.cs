using System;
using VContainer.Unity;
using Internal.Scripts.Core.TimeSystem;
using UnityEngine;

namespace Internal.Scripts.Presenter
{
    public class GameUIPresenter : IInitializable, IDisposable
    {
        private readonly IUICanvas _uiCanvas;
        private readonly ITimeService _timeService;

        private readonly float[] _timeScales = { 1.0f, 2.0f, 3.0f };
        private int _currentTimeScaleIndex = 0;

        // Part Selection Event for MainPresenter to listen to
        public event Action<int> OnPartChanged;

        public GameUIPresenter(IUICanvas uiCanvas, ITimeService timeService)
        {
            _uiCanvas = uiCanvas;
            _timeService = timeService;
        }

        public void Initialize()
        {
            // UI -> Logic
            _uiCanvas.OnPartSelected += HandlePartSelected;
            _uiCanvas.OnTimeSpeedToggleRequested += HandleTimeSpeedToggle;
            _uiCanvas.OnSettingsRequested += HandleSettingsClicked;

            // Logic -> UI
            _timeService.OnDayPassed += HandleDayPassed;
            
            // Sync initial state
            _uiCanvas.UpdateDayText(_timeService.CurrentDay);
            _uiCanvas.UpdateTimeScaleText(_timeService.CurrentTimeScale);
        }

        public void Dispose()
        {
            _uiCanvas.OnPartSelected -= HandlePartSelected;
            _uiCanvas.OnTimeSpeedToggleRequested -= HandleTimeSpeedToggle;
            _uiCanvas.OnSettingsRequested -= HandleSettingsClicked;
            _timeService.OnDayPassed -= HandleDayPassed;
        }

        private void HandlePartSelected(int index)
        {
            Debug.Log($"[GameUIPresenter] Part {index} selected from UI");
            OnPartChanged?.Invoke(index);
        }

        private void HandleTimeSpeedToggle()
        {
            _currentTimeScaleIndex = (_currentTimeScaleIndex + 1) % _timeScales.Length;
            float nextScale = _timeScales[_currentTimeScaleIndex];
            _timeService.SetTimeScale(nextScale);
            _uiCanvas.UpdateTimeScaleText(nextScale);
            Debug.Log($"[GameUIPresenter] TimeScale toggled to: {nextScale}");
        }

        private void HandleSettingsClicked()
        {
            Debug.Log("[GameUIPresenter] Settings menu requested");
        }

        private void HandleDayPassed(int day)
        {
            _uiCanvas.UpdateDayText(day);
        }
    }
}
