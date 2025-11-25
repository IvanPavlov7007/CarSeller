using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SpoilerVariantConfig", menuName = "Configs/Products/Spoiler/Spoiler Variant Config")]
public class SpoilerVariantConfig : ScriptableObject, IVariantConfig
{
    public bool OverrideBase;
    public SpoilerBaseConfig FallbackConfig;
    public SpoilerConfigOverrides Overrides;

    bool IVariantConfig.OverrideBase => OverrideBase;
    IConfigOverrides IVariantConfig.Overrides => Overrides;
    IBaseConfig IVariantConfig.FallbackBase => FallbackConfig;

    public class SpoilerConfigOverrides : IConfigOverrides
    {
        public bool OverrideName;
        [ShowIf("OverrideName")]
        public string Name;
        public bool OverrideSprite;
        [ShowIf("OverrideSprite")]
        public Sprite Sprite;
        public bool OverrideColor;
        [ShowIf("OverrideColor")]
        public Color Color;
        public bool OverrideSize;
        [ShowIf("OverrideSize")]
        public float Size;
    }

}