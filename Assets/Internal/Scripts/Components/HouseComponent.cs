using UnityEngine;

public class HouseComponent : MonoBehaviour
{
    public int HouseId { get; private set; }
    public float CurrentPower { get; private set; }

    private HouseType _houseType = HouseType.None;
    
    
}
