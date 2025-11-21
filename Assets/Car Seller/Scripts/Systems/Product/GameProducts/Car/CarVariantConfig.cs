using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CarVariantConfig : ScriptableObject
{
    public CarBaseConfig baseFallbackConfig;
    public CarFrameVariantConfig carFrameRuntimeConfig;
    public List<PartSlotVariantConfig> slotConfigs;
}