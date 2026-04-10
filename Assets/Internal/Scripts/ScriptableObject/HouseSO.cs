using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HouseSO", menuName = "ScriptableObjects/HouseSO")]
public class HouseSO : ScriptableObject
{
    public List<HouseModel> houseModels;
}

[Serializable]
public class HouseModel
{
    public string houseName;
    public GameObject prefab;
    public float requiredPower; // 이 집이 정상 작동하기 위해 필요한 전력량 (예: 10)
    public float satisfactionFailRate;
}
