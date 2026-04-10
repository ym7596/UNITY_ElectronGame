using UnityEngine;

public interface IPowerConsumer
{
    bool IsConnected { get; }
    float GetRequiredPower();     // 얼마나 필요한가?
    void ReceivePower(float amount); // 전기를 받는다 (여기서 로직 처리)
}
