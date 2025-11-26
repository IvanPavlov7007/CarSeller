using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarBaseConfig", menuName = "Configs/Products/Car/Car Base Config")]
public class CarBaseConfig : SerializedScriptableObject, IBaseConfig
{
    public string Name = "New Car";
    public CarFrameBaseConfig CarFrameBaseConfig;
    [ShowInInspector]
    [OdinSerialize]
    public List<PartSlotBaseConfig> SlotConfigs = new List<PartSlotBaseConfig>();
}