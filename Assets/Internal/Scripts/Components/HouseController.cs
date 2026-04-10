using UnityEngine;

public class HouseController : MonoBehaviour, IPowerConsumer
{
    private string _houseName;
    private HouseType _houseType;
    private int _houseId;

    public bool IsConnected { get; }

    public bool CurrentPower { get; private set; }
    public bool RequiredPower { get; private set; }

    public float GetRequiredPower()
    {
        throw new System.NotImplementedException();
    }

    public void ReceivePower(float amount)
    {
        throw new System.NotImplementedException();
    }
}
