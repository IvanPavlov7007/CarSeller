using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBaseConfig : ScriptableObject
{
    public CarFrameBaseConfig carFrameRuntimeConfig;
    public List<PartSlotBaseConfig> slotConfigs;
}