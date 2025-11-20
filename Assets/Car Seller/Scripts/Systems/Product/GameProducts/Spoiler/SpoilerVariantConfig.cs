using Sirenix.OdinInspector;
using UnityEngine;

public class SpoilerVariantConfig : ScriptableObject
{
    public SpoilerBaseConfig baseFallbackConfig;
    public bool changeColor = true;
    [ShowIf("changeColor")]
    public Color variantColor;
}