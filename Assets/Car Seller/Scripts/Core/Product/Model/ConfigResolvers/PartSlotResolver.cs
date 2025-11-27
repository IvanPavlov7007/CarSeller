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

        //check if any base config is present in either base or variant (via fallback) - otherwise only slot data is resolved
        bool resolveProduct = baseConfig.BaseConfig != null ||
            (variantConfig?.VariantConfig != null && variantConfig.VariantConfig.FallbackBase != null);

        switch (baseConfig.SlotType)
        {
            case PartSlotType.Wheels:
                var wheelRuntimeSlot = CreateWheelRuntimeSlot(baseConfig as WheelSlotBaseConfig);
                if(resolveProduct)
                    wheelRuntimeSlot.wheelConfig = 
                        ResolveWheel(baseConfig as WheelSlotBaseConfig, variantConfig as WheelSlotVariantConfig);
                return wheelRuntimeSlot;
            case PartSlotType.Engine:
                var engineRuntimeSlot = CreateEngineRuntimeSlot(baseConfig as EngineSlotBaseConfig);
                if(resolveProduct)
                    engineRuntimeSlot.engineConfig =
                        ResolveEngine(baseConfig as EngineSlotBaseConfig, variantConfig as EngineSlotVariantConfig);
                return engineRuntimeSlot;

            case PartSlotType.Spoiler:
                var spoilerRuntimeSlot = CreateSpoilerRuntimeSlot(baseConfig as SpoilerSlotBaseConfig);
                if(resolveProduct)
                    spoilerRuntimeSlot.spoilerConfig =
                        ResolveSpoiler(baseConfig as SpoilerSlotBaseConfig, variantConfig as SpoilerSlotVariantConfig);
                return spoilerRuntimeSlot;

            default:
                throw new Exception($"Unknown slot base config type: {baseConfig.GetType().Name}");
        }
    }


    private WheelSlotRuntimeConfig CreateWheelRuntimeSlot(WheelSlotBaseConfig baseCfg)
    {
        var runtimeSlot = new WheelSlotRuntimeConfig
        {
            partSlotData = baseCfg.partSlotData
        };
        return runtimeSlot;
    }

    private EngineSlotRuntimeConfig CreateEngineRuntimeSlot(EngineSlotBaseConfig baseCfg)
    {
        var runtimeSlot = new EngineSlotRuntimeConfig
        {
            partSlotData = baseCfg.partSlotData
        };
        return runtimeSlot;
    }

    private SpoilerSlotRuntimeConfig CreateSpoilerRuntimeSlot(SpoilerSlotBaseConfig baseCfg)
    {
        var runtimeSlot = new SpoilerSlotRuntimeConfig
        {
            partSlotData = baseCfg.partSlotData
        };
        return runtimeSlot;
    }

    private WheelRuntimeConfig ResolveWheel(WheelSlotBaseConfig baseCfg, WheelSlotVariantConfig variantCfg)
    {
        return _resolver.Resolve<
            WheelBaseConfig,
            WheelVariantConfig,
            WheelRuntimeConfig
            >(
                baseCfg.wheelBaseConfig,
                variantCfg?.wheelVariantConfig
            ); ;
    }

    private EngineRuntimeConfig ResolveEngine(EngineSlotBaseConfig baseCfg, EngineSlotVariantConfig variantCfg)
    {
        return _resolver.Resolve<
            EngineBaseConfig,
            EngineVariantConfig,
            EngineRuntimeConfig
            >(
                baseCfg.engineBaseConfig,
                variantCfg?.engineVariantConfig
            );
    }

    private SpoilerRuntimeConfig ResolveSpoiler(SpoilerSlotBaseConfig baseCfg, SpoilerSlotVariantConfig variantCfg)
    {
        return _resolver.Resolve<
                SpoilerBaseConfig,
                SpoilerVariantConfig,
                SpoilerRuntimeConfig
            >(
                baseCfg.spoilerBaseConfig,
                variantCfg?.spoilerVariantConfig
            ); 
    }
}