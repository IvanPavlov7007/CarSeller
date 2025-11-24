using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Frame", menuName = "Configs/Products/Frame/Frame Variant Config")]
public class CarFrameVariantConfig : ScriptableObject, IVariantConfig
{
    public CarFrameBaseConfig BaseConfig;
    public CarFrameVariantOverrides CarFrameVariantOverrides;

    public IBaseConfig FallbackBase => throw new System.NotImplementedException();

    public bool OverrideBase => throw new System.NotImplementedException();

    public IConfigOverrides Overrides => throw new System.NotImplementedException();
}

public class CarFrameVariantOverrides : IConfigOverrides
{
    public bool OverrideName;
    [ShowIf("OverrideName")]
    public string Name;

    public bool OverrideSprite;
    [ShowIf("OverrideSprite")]
    public Sprite Sprite;

    public bool OverrideFrameColor;
    [ShowIf("OverrideFrameColor")]
    public Color FrameColor;

    public bool OverrideWindshieldColor;
    [ShowIf("OverrideWindshieldColor")]
    public Color WindshieldColor;
}