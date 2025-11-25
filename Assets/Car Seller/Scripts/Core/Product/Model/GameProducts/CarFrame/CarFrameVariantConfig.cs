using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Frame", menuName = "Configs/Products/Frame/Frame Variant Config")]
public class CarFrameVariantConfig : ScriptableObject, IVariantConfig
{
    public bool OverrideBase;
    public CarFrameBaseConfig BaseConfig;
    public CarFrameVariantOverrides CarFrameVariantOverrides;

    public IBaseConfig FallbackBase => BaseConfig;

    bool IVariantConfig.ForceFallback => OverrideBase;

    public IConfigOverrides Overrides => CarFrameVariantOverrides;
}

public class CarFrameVariantOverrides : IConfigOverrides
{
    public bool OverrideName;
    [ShowIf("OverrideName")]
    public string Name;

    public bool OverrideFrameColor;
    [ShowIf("OverrideFrameColor")]
    public Color FrameColor;

    public bool OverrideWindshieldColor;
    [ShowIf("OverrideWindshieldColor")]
    public Color WindshieldColor;
}