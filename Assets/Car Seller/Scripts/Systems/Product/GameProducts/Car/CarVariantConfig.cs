using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CarVariantConfig : ScriptableObject
{
    public CarBaseConfig baseFallbackConfig;
    public CarFrameVariantConfig carFrameRuntimeConfig;
    public List<PartSlotVariantConfig> slotConfigs;
}