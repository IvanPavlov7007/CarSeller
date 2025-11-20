using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
public class WheelVariantConfig
{
    WheelBaseConfig baseFallbackConfig;
    public bool changeColor = true;
    [ShowIf("changeColor")]
    public Color variantColor;
}