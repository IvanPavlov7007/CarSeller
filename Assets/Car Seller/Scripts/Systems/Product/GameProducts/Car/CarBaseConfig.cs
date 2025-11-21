using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBaseConfig : ScriptableObject
{
    public string Name = "New Car Type";
    public CarFrameBaseConfig carFrameRuntimeConfig;
    public List<PartSlotBaseConfig> slotConfigs;
}