using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class CarFrameVariantConfig
{
    public CarFrameBaseConfig baseFallbackConfig;

    public bool changeFrameColor = true;
    [ShowIf("changeFrameColor")]
    public Color frameColor;

    public bool changeWindshieldColor = true;
    [ShowIf("changeWindshieldColor")]
    public Color windshieldColor;
}