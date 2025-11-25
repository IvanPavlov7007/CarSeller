using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarVariantConfig", menuName = "Configs/Products/Car/Car Variant Config")]
public class CarVariantConfig : ScriptableObject
{
    public CarBaseConfig baseFallbackConfig;
    public CarFrameVariantConfig carFrameRuntimeConfig;
    public List<PartSlotVariantConfig> slotConfigs;
}