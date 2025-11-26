using NUnit.Framework;
using System;
using UnityEngine;

//Todo maybe move type switch to classes themselves

public class PartSlotResolver
{
    private readonly GenericConfigResolver _resolver;

    public PartSlotResolver(GenericConfigResolver resolver)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseConfig"></param>
    /// <param name="variantConfig"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    internal PartSlotRuntimeConfig Resolve(PartSlotBaseConfig baseConfig, PartSlotVariantConfig variantConfig)
    {
        if (baseConfig == null) throw new ArgumentNullException(nameof(baseConfig));
        if(variantConfig != null)
        Debug.Assert(baseConfig.SlotType == variantConfig?.SlotType,
            $"Mismatched slot types: base '{baseConfig.SlotType}', variant '{variantConfig?.SlotType}'");

        switch (baseConfig.SlotType)
        {
            case PartSlotType.Wheels:
                return ResolveWheel(baseConfig as WheelSlotBaseConfig, variantConfig as WheelSlotVariantConfig);

            case PartSlotType.Engine:
                return ResolveEngine(baseConfig as EngineSlotBaseConfig, variantConfig as EngineSlotVariantConfig);

            case PartSlotType.Spoiler:
                return ResolveSpoiler(baseConfig as SpoilerSlotBaseConfig, variantConfig as SpoilerSlotVariantConfig);

            default:
                throw new Exception($"Unknown slot base config type: {baseConfig.GetType().Name}");
        }
    }

    private WheelSlotRuntimeConfig ResolveWheel(WheelSlotBaseConfig baseCfg, WheelSlotVariantConfig variantCfg)
    {
        var runtimeSlot = new WheelSlotRuntimeConfig
        {
            partSlotData = baseCfg.partSlotData
        };
        if (baseCfg.BaseConfig != null || variantCfg?.VariantConfig != null)
        {
        runtimeSlot.wheelConfig = _resolver.Resolve<
            WheelBaseConfig,
            WheelVariantConfig,
            WheelRuntimeConfig
            >(
                baseCfg.wheelBaseConfig,
                variantCfg?.wheelVariantConfig
            );
        }
        return runtimeSlot;
    }

    private EngineSlotRuntimeConfig ResolveEngine(EngineSlotBaseConfig baseCfg, EngineSlotVariantConfig variantCfg)
    {
        var runtimeSlot = new EngineSlotRuntimeConfig
        {
            partSlotData = baseCfg.partSlotData
        };

        if (baseCfg.BaseConfig != null || variantCfg?.VariantConfig != null)
        {
        runtimeSlot.engineConfig = _resolver.Resolve<
            EngineBaseConfig,
            EngineVariantConfig,
            EngineRuntimeConfig
            >(
                baseCfg.engineBaseConfig,
                variantCfg?.engineVariantConfig
            );
        }

        return runtimeSlot;
    }

    private SpoilerSlotRuntimeConfig ResolveSpoiler(SpoilerSlotBaseConfig baseCfg, SpoilerSlotVariantConfig variantCfg)
    {
        var runtimeSlot = new SpoilerSlotRuntimeConfig
        {
            partSlotData = baseCfg.partSlotData
        };

        if (baseCfg.BaseConfig != null || variantCfg?.VariantConfig != null)
        {
            runtimeSlot.spoilerConfig = _resolver.Resolve<
                SpoilerBaseConfig,
                SpoilerVariantConfig,
                SpoilerRuntimeConfig
            >(
                baseCfg.spoilerBaseConfig,
                variantCfg?.spoilerVariantConfig
            );
        }

        return runtimeSlot;
    }
}