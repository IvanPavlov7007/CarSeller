using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EngineVariantConfig", menuName = "Configs/Products/Engine/Engine Variant Config")]
public class EngineVariantConfig : ScriptableObject, IVariantConfig
{
    public bool OverrideBase;
    public EngineBaseConfig FallbackBase;
    public ConfigOverrides Overrides;

    bool IVariantConfig.ForceFallback => OverrideBase;
    IConfigOverrides IVariantConfig.Overrides => Overrides;
    IBaseConfig IVariantConfig.FallbackBase => FallbackBase;

    [Serializable]
    public class ConfigOverrides : IConfigOverrides
    {
        public bool OverrideName;
        [ShowIf("OverrideName")]
        public string Name;

        public bool OverrideLevel;
        [ShowIf("OverrideLevel")]
        public int Level = 1;

    }
}