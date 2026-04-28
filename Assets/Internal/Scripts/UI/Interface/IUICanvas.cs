using System;

public interface IUICanvas 
{
    // Events from UI to Presenter
    event Action<int> OnPartSelected; // index: 0, 1, 2
    event Action OnTimeSpeedToggleRequested;
    event Action OnSettingsRequested;

    // Methods to update UI from Presenter
    void UpdateDayText(int day);
    void UpdateTimeScaleText(float scale);
}
