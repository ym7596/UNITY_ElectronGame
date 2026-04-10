using System;
using UnityEngine;
using VContainer.Unity;

namespace Internal.Scripts.Core.TimeSystem
{
    public class TimeManager : ITimeService, ITickable
    {
        // 설정 값
        private const float SECONDS_PER_DAY = 30f; // 하루는 실제 시간 30초 (테스트용)
        private const float MIN_SPAWN_INTERVAL = 3f;
        private const float MAX_SPAWN_INTERVAL = 7f;

        // 상태 변수
        public float TotalPlayTime { get; private set; }
        public int CurrentDay { get; private set; } = 1;
        public float NormalizedDayTime => (TotalPlayTime % SECONDS_PER_DAY) / SECONDS_PER_DAY;
        public bool IsPaused { get; private set; } = false;
        public float CurrentTimeScale { get; private set; } = 1.0f;

        // 이벤트
        public event Action<int> OnDayPassed;
        public event Action OnHouseSpawnRequested;

        // 집 스폰 타이머 관련
        private float _spawnAccumulator;
        private float _nextSpawnTime;

        public TimeManager()
        {
            SetNextSpawnInterval();
        }

        public void Tick()
        {
            if (IsPaused) return;

            float scaledDeltaTime = Time.deltaTime * CurrentTimeScale;
            TotalPlayTime += scaledDeltaTime;

            // 1. 날짜 경과 체크
            int dayCalculated = (int)(TotalPlayTime / SECONDS_PER_DAY) + 1;
            if (dayCalculated > CurrentDay)
            {
                CurrentDay = dayCalculated;
                OnDayPassed?.Invoke(CurrentDay);
                Debug.Log($"[Time] Day {CurrentDay} Started!");
            }

            // 2. 집 스폰 로직 (프레젠터나 Spawner에서 이 이벤트를 구독)
            _spawnAccumulator += scaledDeltaTime;
            if (_spawnAccumulator >= _nextSpawnTime)
            {
                OnHouseSpawnRequested?.Invoke();
                _spawnAccumulator = 0;
                SetNextSpawnInterval();
                Debug.Log("[Time] House Spawn Requested");
            }
        }

        public void SetTimeScale(float scale)
        {
            CurrentTimeScale = Mathf.Max(0, scale);
            IsPaused = (CurrentTimeScale <= 0);
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;
            if (IsPaused)
            {
                // 시간을 멈추지만 스케일 값은 기억해두고 싶을 때를 위해 0으로 만들지 않고 체크만 함
            }
        }

        private void SetNextSpawnInterval()
        {
            // 난이도(날짜 등)에 따라 간격이 점점 짧아지게 설계 가능
            float difficultyBonus = Mathf.Max(0.5f, 1.0f - (CurrentDay * 0.05f)); 
            _nextSpawnTime = UnityEngine.Random.Range(MIN_SPAWN_INTERVAL, MAX_SPAWN_INTERVAL) * difficultyBonus;
        }
    }
}
