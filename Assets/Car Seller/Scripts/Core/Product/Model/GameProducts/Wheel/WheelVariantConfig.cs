using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "WheelVariantConfig", menuName = "Configs/Products/Wheel/Wheel Variant Config")]
public class WheelVariantConfig : ScriptableObject, IVariantConfig
{
    public WheelBaseConfig BaseFallback;
    public bool ForceFallback;
    public WheelOverrides Overrides;

    public IBaseConfig FallbackBase => BaseFallback;
    bool IVariantConfig.ForceFallback => ForceFallback;
    IConfigOverrides IVariantConfig.Overrides => Overrides;
}

[Serializable]
public class WheelOverrides : IConfigOverrides
{
    public bool OverrideName;
    [ShowIf("OverrideName")]    
    public string Name;

    public bool OverrideColor;
    [ShowIf("OverrideColor")]
    public Color Color;

    public bool OverrideSideViewSize;
    [ShowIf("OverrideSideViewSize")]
    public float SideViewSize;

    public bool OverrideTopViewSize;
    [ShowIf("OverrideTopViewSize")]
    public float TopViewSize;
}