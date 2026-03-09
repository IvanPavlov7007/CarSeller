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
    public bool OverrideSpeed;
    [ShowIf("OverrideSpeed")]
    public float Speed;
    public bool OverrideAcceleration;
    [ShowIf("OverrideAcceleration")]
    public float Acceleration;

    public CarFrameVariantConfig carFrameRuntimeConfig;
    [OdinSerialize]
    public List<PartSlotVariantConfig> slotConfigs = new List<PartSlotVariantConfig>();


    [BoxGroup("Simplified params")]
    public bool OverrideColor;
    [ShowIf("OverrideColor")]
    [BoxGroup("Simplified params")]
    public CarColor Color;
    [BoxGroup("Simplified params")]
    public bool OverrideType;
    [ShowIf("OverrideType")]
    [BoxGroup("Simplified params")]
    public CarType Type;
    public bool OverrideRarity;
    [ShowIf("OverrideRarity")]
    [BoxGroup("Simplified params")]
    public CarRarity Rarity;
}