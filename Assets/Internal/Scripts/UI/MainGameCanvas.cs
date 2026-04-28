using System;
using UnityEngine;
using TMPro;

namespace Internal.Scripts.UI
{
    public class MainGameCanvas : MonoBehaviour, IUICanvas
    {
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI timeScaleText;
        
        private int _currentSelectedPartIndex = 0;

        public event Action<int> OnPartSelected;
        public event Action OnTimeSpeedToggleRequested;
        public event Action OnSettingsRequested;

        // 이 메서드들은 인스펙터에서 버튼의 OnClick 이벤트에 연결하거나 
        // Awake/Start에서 버튼 컴포넌트를 직접 찾아 리스너를 등록하여 호출하세요.
        
        public void SelectPart(int index) => OnPartSelected?.Invoke(index);
        public void ToggleTimeSpeed() => OnTimeSpeedToggleRequested?.Invoke();
        public void OpenSettings() => OnSettingsRequested?.Invoke();

        public void UpdateDayText(int day)
        {
            // TODO: UI Text/TextMeshPro 업데이트 로직
            // Debug.Log($"[UI View] Day updated to: {day}");
        }

        public void UpdateTimeScaleText(float scale)
        {
            // TODO: UI Text/TextMeshPro 업데이트 로직
            // Debug.Log($"[UI View] TimeScale updated to: {scale}x");
        }
    }
}
