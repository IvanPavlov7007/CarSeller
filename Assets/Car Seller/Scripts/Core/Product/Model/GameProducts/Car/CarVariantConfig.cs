using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarVariantConfig", menuName = "Configs/Products/Car/Car Variant Config")]
public class CarVariantConfig : SerializedScriptableObject, IVariantConfig
{
    public CarBaseConfig baseFallbackConfig;
    public bool OverrideBasePrice;
    [ShowIf("OverrideBasePrice")]
    public float BasePrice;
    public CarFrameVariantConfig carFrameRuntimeConfig;
    [OdinSerialize]
    public List<PartSlotVariantConfig> slotConfigs = new List<PartSlotVariantConfig>();
}