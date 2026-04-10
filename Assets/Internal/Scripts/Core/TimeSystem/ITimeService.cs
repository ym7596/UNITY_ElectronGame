using System;

namespace Internal.Scripts.Core.TimeSystem
{
    public interface ITimeService
    {
        // 시간 상태
        float TotalPlayTime { get; }
        int CurrentDay { get; }
        float NormalizedDayTime { get; } // 0~1 사이의 하루 진행도
        bool IsPaused { get; }
        float CurrentTimeScale { get; }

        // 컨트롤
        void SetTimeScale(float scale);
        void TogglePause();
        
        // 이벤트
        event Action<int> OnDayPassed; // 날짜가 바뀔 때 호출
        event Action OnHouseSpawnRequested; // 집 스폰이 필요할 때 호출
    }
}
