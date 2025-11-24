using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarBaseConfig", menuName = "Configs/Products/Car/Car Base Config")]
public class CarBaseConfig : ScriptableObject, IBaseConfig
{
    public string Name = "New Car";
    public CarFrameBaseConfig CarFrameRuntimeConfig;
    public List<PartSlotBaseConfig> SlotConfigs;
}