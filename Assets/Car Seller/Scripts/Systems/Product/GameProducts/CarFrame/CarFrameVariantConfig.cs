using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class CarFrameVariantConfig : ScriptableObject
{
    public CarFrameBaseConfig baseFallbackConfig;

    public bool changeFrameColor = true;
    [ShowIf("changeFrameColor")]
    public Color frameColor;

    public bool changeWindshieldColor = true;
    [ShowIf("changeWindshieldColor")]
    public Color windshieldColor;
}