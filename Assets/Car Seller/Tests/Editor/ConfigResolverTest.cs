using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.TestTools;

public class ConfigResolverTest
{
    GenericConfigResolver genericResolver = new GenericConfigResolver();
    CarResolver carResolver = new CarResolver();

    [Test]
    public void WheelRuntimeConfig_FromBaseConfig()
    {
        var wheelBaseConfig = createWheelBaseConfig(WheelType.OffRoad);

        var runtime = genericResolver.Resolve<WheelBaseConfig, WheelVariantConfig, WheelRuntimeConfig>(wheelBaseConfig, null);

        Assert.IsTrue(wheelBaseConfig.WheelType == runtime.WheelType && wheelBaseConfig.Name == runtime.Name);
    }

    [Test]
    public void WheelRuntimeConfig_FromVariantOnly()
    {
        var sportBase = createWheelBaseConfig(WheelType.Sport);
        var wheelVariantConfig = createWheelVariantConfig(new WheelOverrides
        {
            Color = Color.red,
            OverrideColor = true,
        }, sportBase, false);

        var runtime = genericResolver.Resolve<WheelBaseConfig, WheelVariantConfig, WheelRuntimeConfig>
            (null, wheelVariantConfig);

        Assert.IsTrue(sportBase.WheelType == runtime.WheelType &&
            wheelVariantConfig.Overrides.Color == runtime.Color);
    }

    [Test]  
    public void WheelRuntimeConfig_Variant_Base_No_Fallback()
    {
        var wheelBaseConfig = createWheelBaseConfig(WheelType.OffRoad);
        var wheelVariantConfig = createWheelVariantConfig(new WheelOverrides { Color = Color.red, OverrideColor = true,
            OverrideSideViewSize = true, SideViewSize = 2f });

        var runtime = genericResolver.Resolve<WheelBaseConfig, WheelVariantConfig, WheelRuntimeConfig>
            (wheelBaseConfig, wheelVariantConfig);

        Assert.IsTrue(wheelBaseConfig.Name == runtime.Name && 
            wheelBaseConfig.SideViewSize != runtime.SideViewSize &&
            wheelVariantConfig.Overrides.Color == runtime.Color);
    }

    [Test]
    public void WheelRuntimeConfig_Variant_Base_Fallback()
    {
        var sportBase = createWheelBaseConfig(WheelType.Sport);
        var offroadBase = createWheelBaseConfig(WheelType.OffRoad);
        var wheelVariantConfig = createWheelVariantConfig(new WheelOverrides
        {
            Color = Color.red,
            OverrideColor = true,
        }, sportBase, true);

        var runtime = genericResolver.Resolve<WheelBaseConfig, WheelVariantConfig, WheelRuntimeConfig>
            (offroadBase, wheelVariantConfig);

        Assert.IsTrue(sportBase.WheelType == runtime.WheelType &&
            wheelVariantConfig.Overrides.Color == runtime.Color);
    }

    #region ChatGPTGenerated

    // CAR VARIANT + WHEEL SLOT SCENARIOS

    [Test]
    public void CarResolver_WheelSlot_ForceFallback_UsesVariantFallbackAndOverridesColor()
    {
        var offroadWheelBase = createWheelBaseConfig(WheelType.OffRoad);
        var sportWheelBase = createWheelBaseConfig(WheelType.Sport);

        var wheelVariant = createWheelVariantConfig(new WheelOverrides
        {
            OverrideColor = true,
            Color = Color.magenta
        }, sportWheelBase, true);

        var baseWheelSlot = createBasePartSlot(PartSlotType.Wheels, offroadWheelBase);
        var variantWheelSlot = createVariantSlot(PartSlotType.Wheels, wheelVariant);

        var frameBase = createCarFrameBaseConfig();
        var carBase = createCarBaseConfig(frameBase, new List<PartSlotBaseConfig> { baseWheelSlot });
        var carVariant = createCarVariantConfig(null, new List<PartSlotVariantConfig> { variantWheelSlot });

        var runtime = carResolver.Resolve(carBase, carVariant);

        Assert.IsNotNull(runtime);
        Assert.AreEqual(1, runtime.SlotConfigs.Count);

        var wheelSlotRuntime = runtime.SlotConfigs[0] as WheelSlotRuntimeConfig;
        Assert.IsNotNull(wheelSlotRuntime);

        var wheelRuntime = wheelSlotRuntime.wheelConfig;
        Assert.IsNotNull(wheelRuntime);

        // Should use sport fallback (variant BaseFallback) instead of offroad base
        Assert.AreEqual(sportWheelBase.WheelType, wheelRuntime.WheelType, "Wheel type should come from fallback due to ForceFallback");

        // Color override applied
        Assert.AreEqual(wheelVariant.Overrides.Color, wheelRuntime.Color, "Color should be overridden by variant");
    }

    [Test]
    public void CarResolver_WheelSlot_NoForceFallback_OverridesColorOnly()
    {
        var offroadWheelBase = createWheelBaseConfig(WheelType.OffRoad);
        var sportWheelBase = createWheelBaseConfig(WheelType.Sport); // provided as potential fallback but ForceFallback = false

        var wheelVariant = createWheelVariantConfig(new WheelOverrides
        {
            OverrideColor = true,
            Color = Color.green
        }, sportWheelBase, false); // ForceFallback = false

        var baseWheelSlot = createBasePartSlot(PartSlotType.Wheels, offroadWheelBase);
        var variantWheelSlot = createVariantSlot(PartSlotType.Wheels, wheelVariant);

        var frameBase = createCarFrameBaseConfig();
        var carBase = createCarBaseConfig(frameBase, new List<PartSlotBaseConfig> { baseWheelSlot });
        var carVariant = createCarVariantConfig(null, new List<PartSlotVariantConfig> { variantWheelSlot });

        var runtime = carResolver.Resolve(carBase, carVariant);

        Assert.IsNotNull(runtime);
        Assert.AreEqual(1, runtime.SlotConfigs.Count);

        var wheelSlotRuntime = runtime.SlotConfigs[0] as WheelSlotRuntimeConfig;
        Assert.IsNotNull(wheelSlotRuntime);

        var wheelRuntime = wheelSlotRuntime.wheelConfig;
        Assert.IsNotNull(wheelRuntime);

        Assert.AreEqual(offroadWheelBase.WheelType, wheelRuntime.WheelType);
        Assert.AreEqual(wheelVariant.Overrides.Color, wheelRuntime.Color);
    }

    // ENGINE TESTS

    [Test]
    public void EngineRuntimeConfig_FromBaseOnly()
    {
        var engineBase = createEngineBaseConfig(3);
        var runtime = genericResolver.Resolve<EngineBaseConfig, EngineVariantConfig, EngineRuntimeConfig>(engineBase, null);
        Assert.IsNotNull(runtime);
        Assert.AreEqual(engineBase.Level, runtime.Level);
    }

    [Test]
    public void EngineRuntimeConfig_VariantOverridesLevel_NoFallback()
    {
        var engineBase = createEngineBaseConfig(2);
        var overrides = new EngineVariantConfig.ConfigOverrides
        {
            OverrideLevel = true,
            Level = 5
        };
        var variant = createEngineVariantConfig(overrides, engineBase, false);
        var runtime = genericResolver.Resolve<EngineBaseConfig, EngineVariantConfig, EngineRuntimeConfig>(engineBase, variant);
        Assert.AreEqual(5, runtime.Level);
    }

    [Test]
    public void EngineRuntimeConfig_VariantForceFallback_UsesFallbackBaseLevel()
    {
        var primaryBase = createEngineBaseConfig(2);
        var fallbackBase = createEngineBaseConfig(7);
        var overrides = new EngineVariantConfig.ConfigOverrides
        {
            OverrideLevel = false
        };
        var variant = createEngineVariantConfig(overrides, fallbackBase, true);
        var runtime = genericResolver.Resolve<EngineBaseConfig, EngineVariantConfig, EngineRuntimeConfig>(primaryBase, variant);
        Assert.AreEqual(fallbackBase.Level, runtime.Level);
    }

    // SPOILER TESTS

    [Test]
    public void SpoilerRuntimeConfig_FromBaseOnly()
    {
        var spoilerBase = createSpoilerBaseConfig();
        var runtime = genericResolver.Resolve<SpoilerBaseConfig, SpoilerVariantConfig, SpoilerRuntimeConfig>(spoilerBase, null);
        Assert.IsNotNull(runtime);
        Assert.AreEqual(spoilerBase.Color, runtime.Color);
        Assert.AreEqual(spoilerBase.Size, runtime.Size);
    }

    [Test]
    public void SpoilerRuntimeConfig_VariantOverridesColorAndSize_NoFallback()
    {
        var spoilerBase = createSpoilerBaseConfig();
        var overrides = new SpoilerVariantConfig.SpoilerConfigOverrides
        {
            OverrideColor = true,
            Color = Color.yellow,
            OverrideSize = true,
            Size = 3f
        };
        var variant = createSpoilerVariantConfig(overrides, spoilerBase, false);
        var runtime = genericResolver.Resolve<SpoilerBaseConfig, SpoilerVariantConfig, SpoilerRuntimeConfig>(spoilerBase, variant);
        Assert.AreEqual(Color.yellow, runtime.Color);
        Assert.AreEqual(3f, runtime.Size);
    }

    [Test]
    public void SpoilerRuntimeConfig_VariantForceFallback_UsesFallbackBaseValues()
    {
        var primaryBase = createSpoilerBaseConfig();
        primaryBase.Color = Color.white;
        primaryBase.Size = 1f;

        var fallbackBase = createSpoilerBaseConfig();
        fallbackBase.Color = Color.cyan;
        fallbackBase.Size = 2.5f;

        var overrides = new SpoilerVariantConfig.SpoilerConfigOverrides
        {
            OverrideColor = false,
            OverrideSize = false
        };
        var variant = createSpoilerVariantConfig(overrides, fallbackBase, true);
        var runtime = genericResolver.Resolve<SpoilerBaseConfig, SpoilerVariantConfig, SpoilerRuntimeConfig>(primaryBase, variant);
        Assert.AreEqual(fallbackBase.Color, runtime.Color);
        Assert.AreEqual(fallbackBase.Size, runtime.Size);
    }

    // CAR FRAME TESTS

    [Test]
    public void CarFrameRuntimeConfig_VariantOverridesFrameColor_NoOverrideBase()
    {
        var frameBase = createCarFrameBaseConfig();
        frameBase.FrameColor = Color.white;
        var overrides = new CarFrameVariantOverrides
        {
            OverrideFrameColor = true,
            FrameColor = Color.red
        };
        var frameVariant = createCarFrameVariantConfig(overrides, frameBase, false);

        var carBase = createCarBaseConfig(frameBase, new List<PartSlotBaseConfig>());
        var carVariant = createCarVariantConfig(frameVariant, new List<PartSlotVariantConfig>());

        var runtime = carResolver.Resolve(carBase, carVariant);
        Assert.IsNotNull(runtime);
        Assert.AreEqual(Color.red, runtime.CarFrameRuntimeConfig.FrameColor);
    }

    [Test]
    public void CarFrameRuntimeConfig_VariantOverrideBase_UsesVariantBaseConfigFrameColor()
    {
        var originalBase = createCarFrameBaseConfig();
        originalBase.FrameColor = Color.white;

        var variantBase = createCarFrameBaseConfig();
        variantBase.FrameColor = Color.magenta;

        var overrides = new CarFrameVariantOverrides
        {
            OverrideFrameColor = false
        };

        var frameVariant = createCarFrameVariantConfig(overrides, variantBase, true);

        var carBase = createCarBaseConfig(originalBase, new List<PartSlotBaseConfig>());
        var carVariant = createCarVariantConfig(frameVariant, new List<PartSlotVariantConfig>());

        var runtime = carResolver.Resolve(carBase, carVariant);
        Assert.IsNotNull(runtime);
        Assert.AreEqual(variantBase.FrameColor, runtime.CarFrameRuntimeConfig.FrameColor);
    }

    // INTEGRATED CAR RESOLUTION (MULTIPLE SLOTS MIXED)

    [Test]
    public void CarResolver_MixedSlots_VariantEngineFallback_WheelOverride_SpoilerBaseOnly()
    {
        // Base configs
        var frameBase = createCarFrameBaseConfig();
        frameBase.FrameColor = Color.gray;

        var engineBasePrimary = createEngineBaseConfig(2);
        var engineBaseFallback = createEngineBaseConfig(9);

        var wheelBase = createWheelBaseConfig(WheelType.OffRoad);
        var spoilerBase = createSpoilerBaseConfig();
        spoilerBase.Color = Color.white;

        // Base slots
        var engineSlotBase = createBasePartSlot(PartSlotType.Engine, engineBasePrimary);
        var wheelSlotBase = createBasePartSlot(PartSlotType.Wheels, wheelBase);
        var spoilerSlotBase = createBasePartSlot(PartSlotType.Spoiler, spoilerBase);

        // Variant engine (force fallback)
        var engineOverrides = new EngineVariantConfig.ConfigOverrides
        {
            OverrideLevel = false
        };
        var engineVariant = createEngineVariantConfig(engineOverrides, engineBaseFallback, true);

        // Variant wheel (override color, no force fallback)
        var wheelOverrides = new WheelOverrides
        {
            OverrideColor = true,
            Color = Color.blue
        };
        var wheelVariant = createWheelVariantConfig(wheelOverrides, wheelBase, false);

        var engineVariantSlot = createVariantSlot(PartSlotType.Engine, engineVariant);
        var wheelVariantSlot = createVariantSlot(PartSlotType.Wheels, wheelVariant);
        // Spoiler has no variant (base only)

        var carBase = createCarBaseConfig(frameBase, new List<PartSlotBaseConfig>
        {
            engineSlotBase, wheelSlotBase, spoilerSlotBase
        });

        var carVariant = createCarVariantConfig(null, new List<PartSlotVariantConfig>
        {
            engineVariantSlot, wheelVariantSlot
        });

        var runtime = carResolver.Resolve(carBase, carVariant);

        Assert.IsNotNull(runtime);
        Assert.AreEqual(3, runtime.SlotConfigs.Count, "Should have 3 runtime slots (engine, wheels, spoiler)");

        EngineSlotRuntimeConfig engineRuntimeSlot = null;
        WheelSlotRuntimeConfig wheelRuntimeSlot = null;
        SpoilerSlotRuntimeConfig spoilerRuntimeSlot = null;

        foreach (var slot in runtime.SlotConfigs)
        {
            switch (slot.SlotType)
            {
                case PartSlotType.Engine: engineRuntimeSlot = slot as EngineSlotRuntimeConfig; break;
                case PartSlotType.Wheels: wheelRuntimeSlot = slot as WheelSlotRuntimeConfig; break;
                case PartSlotType.Spoiler: spoilerRuntimeSlot = slot as SpoilerSlotRuntimeConfig; break;
            }
        }

        Assert.IsNotNull(engineRuntimeSlot);
        Assert.IsNotNull(wheelRuntimeSlot);
        Assert.IsNotNull(spoilerRuntimeSlot);

        // Engine level must come from fallback base due to force fallback
        Assert.AreEqual(engineBaseFallback.Level, engineRuntimeSlot.engineConfig.Level);

        // Wheel type should remain base (no force fallback), color overridden
        Assert.AreEqual(wheelBase.WheelType, wheelRuntimeSlot.wheelConfig.WheelType);
        Assert.AreEqual(Color.blue, wheelRuntimeSlot.wheelConfig.Color);

        // Spoiler should be identical to base (no variant)
        Assert.AreEqual(spoilerBase.Color, spoilerRuntimeSlot.spoilerConfig.Color);
        Assert.AreEqual(spoilerBase.Size, spoilerRuntimeSlot.spoilerConfig.Size);
    }

    // NULL / VALIDATION TESTS

    [Test]
    public void CarResolver_NoFrameBaseAndNoVariantFrame_Throws()
    {
        var carBase = createCarBaseConfig(null, new List<PartSlotBaseConfig>());
        var carVariant = createCarVariantConfig(null, null);

        Assert.Throws<System.Exception>(() => carResolver.Resolve(carBase, carVariant),
            "Expected exception when both base and variant frame configs are null.");
    }

    [Test]
    public void CarResolver_BaseWithSlots_NoVariantSlots_CreatesRuntimeSlotsBaseOccupied()
    {
        var frameBase = createCarFrameBaseConfig();
        frameBase.FrameColor = Color.black;

        var wheelBase = createWheelBaseConfig(WheelType.Sport);
        var engineBase = createEngineBaseConfig(4);
        var spoilerBase = createSpoilerBaseConfig();

        var wheelSlotBase = createBasePartSlot(PartSlotType.Wheels, wheelBase);
        var engineSlotBase = createBasePartSlot(PartSlotType.Engine, engineBase);
        var spoilerSlotBase = createBasePartSlot(PartSlotType.Spoiler, spoilerBase);

        var carBase = createCarBaseConfig(frameBase, new List<PartSlotBaseConfig> { wheelSlotBase, engineSlotBase, spoilerSlotBase });

        CarVariantConfig carVariant = null; // No variants provided, slots are "unoccupied" by products.

        var runtime = carResolver.Resolve(carBase, carVariant);

        Assert.IsNotNull(runtime);
        Assert.IsNotNull(runtime.CarFrameRuntimeConfig);
        Assert.AreEqual(3, runtime.SlotConfigs.Count);

        foreach (var slot in runtime.SlotConfigs)
        {
            switch (slot.SlotType)
            {
                case PartSlotType.Wheels:
                    Assert.IsNotNull(((WheelSlotRuntimeConfig)slot).wheelConfig);
                    break;
                case PartSlotType.Engine:
                    Assert.IsTrue(((EngineSlotRuntimeConfig)slot).partSlotData.Hidden, "Engine slot should be hidden by base setup.");
                    Assert.IsNotNull(((EngineSlotRuntimeConfig)slot).engineConfig);
                    break;
                case PartSlotType.Spoiler:
                    Assert.IsNotNull(((SpoilerSlotRuntimeConfig)slot).spoilerConfig);
                    break;
            }
        }
    }

    [Test]
    public void CarResolver_BaseWithSlots_NoVariantSlots_CreatesRuntimeSlotsUnoccupied()
    {
        var frameBase = createCarFrameBaseConfig();
        frameBase.FrameColor = Color.black;
        var wheelSlotBase = createBasePartSlot(PartSlotType.Wheels, null);
        var engineSlotBase = createBasePartSlot(PartSlotType.Engine, null);
        var spoilerSlotBase = createBasePartSlot(PartSlotType.Spoiler, null);

        var carBase = createCarBaseConfig(frameBase, new List<PartSlotBaseConfig> { wheelSlotBase, engineSlotBase, spoilerSlotBase });

        CarVariantConfig carVariant = null; // No variants provided, slots are "unoccupied" by products.

        var runtime = carResolver.Resolve(carBase, carVariant);

        Assert.IsNotNull(runtime);
        Assert.IsNotNull(runtime.CarFrameRuntimeConfig);
        Assert.AreEqual(3, runtime.SlotConfigs.Count);

        foreach (var slot in runtime.SlotConfigs)
        {
            switch (slot.SlotType)
            {
                case PartSlotType.Wheels:
                    Assert.IsNull(((WheelSlotRuntimeConfig)slot).wheelConfig);
                    break;
                case PartSlotType.Engine:
                    Assert.IsNull(((EngineSlotRuntimeConfig)slot).engineConfig);
                    break;
                case PartSlotType.Spoiler:
                    Assert.IsNull(((SpoilerSlotRuntimeConfig)slot).spoilerConfig);
                    break;
            }
        }
    }

    [Test]
    public void CarResolver_BaseWithEmptySlots_VariantSlotsMixedFallback()
    {
        var frameBase = createCarFrameBaseConfig();
        frameBase.FrameColor = Color.black;

        var wheelBase = createWheelBaseConfig(WheelType.Sport);
        var engineBase = createEngineBaseConfig(4);
        var spoilerBase = createSpoilerBaseConfig();

        var wheelVariant = createWheelVariantConfig(new WheelOverrides
        {
            OverrideColor = true,
            Color = Color.green
        }, wheelBase, false);

        var engineVariant = createEngineVariantConfig(new EngineVariantConfig.ConfigOverrides
        {
            OverrideLevel = false
        }, engineBase, true);

        var spoilerVariant = createSpoilerVariantConfig(new SpoilerVariantConfig.SpoilerConfigOverrides
        {
            OverrideSize = true,
            Size = 5f
        }, spoilerBase, false);

        var carVariant = createCarVariantConfig(null, new List<PartSlotVariantConfig>
        {
            createVariantSlot(PartSlotType.Wheels, wheelVariant),
            createVariantSlot(PartSlotType.Engine, engineVariant),
            createVariantSlot(PartSlotType.Spoiler, spoilerVariant)
        });

        var wheelSlotBase = createBasePartSlot(PartSlotType.Wheels, null);
        var engineSlotBase = createBasePartSlot(PartSlotType.Engine, null);
        var spoilerSlotBase = createBasePartSlot(PartSlotType.Spoiler, null);

        var carBase = createCarBaseConfig(frameBase, new List<PartSlotBaseConfig> { wheelSlotBase, engineSlotBase, spoilerSlotBase });

        var runtime = carResolver.Resolve(carBase, carVariant);

        Assert.IsNotNull(runtime);
        Assert.IsNotNull(runtime.CarFrameRuntimeConfig);
        Assert.AreEqual(3, runtime.SlotConfigs.Count);

        foreach (var slot in runtime.SlotConfigs)
        {
            switch (slot.SlotType)
            {
                case PartSlotType.Wheels:
                    Assert.IsNotNull(((WheelSlotRuntimeConfig)slot).wheelConfig);
                    break;
                case PartSlotType.Engine:
                    Assert.IsTrue(((EngineSlotRuntimeConfig)slot).partSlotData.Hidden, "Engine slot should be hidden by base setup.");
                    Assert.IsNotNull(((EngineSlotRuntimeConfig)slot).engineConfig);
                    break;
                case PartSlotType.Spoiler:
                    Assert.IsNotNull(((SpoilerSlotRuntimeConfig)slot).spoilerConfig);
                    break;
            }
        }
    }
    #endregion

    #region CreationMethods
    WheelBaseConfig createWheelBaseConfig(WheelType wheelType)
    {
        var config = ScriptableObject.CreateInstance<WheelBaseConfig>();
        config.name = "Test Wheel Base Config";
        config.Color = Color.white;
        config.SideViewSize = 1f;
        config.TopViewSize = 0.5f;
        config.WheelType = wheelType;
        return config;
    }

    WheelVariantConfig createWheelVariantConfig(WheelOverrides overrides, WheelBaseConfig baseConfig = null, bool forceUseFallback = false)
    {
        var config = ScriptableObject.CreateInstance<WheelVariantConfig>();
        config.name = "Test Wheel Variant Config";
        config.Overrides = overrides;
        config.BaseFallback = baseConfig;
        config.ForceFallback = forceUseFallback;
        return config;
    }

    EngineBaseConfig createEngineBaseConfig(int level)
    {
        var config = ScriptableObject.CreateInstance<EngineBaseConfig>();
        config.name = "Test Engine Base Config";
        config.Level = level;
        return config;
    }

    EngineVariantConfig createEngineVariantConfig(EngineVariantConfig.ConfigOverrides overrides, EngineBaseConfig baseConfig = null, bool forceUseFallback = false)
    {
        var config = ScriptableObject.CreateInstance<EngineVariantConfig>();
        config.name = "Test Engine Variant Config";
        config.Overrides = overrides;
        config.FallbackBase = baseConfig;
        config.OverrideBase = forceUseFallback;
        return config;
    }

    SpoilerBaseConfig createSpoilerBaseConfig()
    {
        var config = ScriptableObject.CreateInstance<SpoilerBaseConfig>();
        config.name = "Test Spoiler Base Config";
        config.Color = Color.white;
        config.Size = 1f;
        return config;
    }

    SpoilerVariantConfig createSpoilerVariantConfig(SpoilerVariantConfig.SpoilerConfigOverrides overrides, SpoilerBaseConfig baseConfig = null, bool forceUseFallback = false)
    {
        var config = ScriptableObject.CreateInstance<SpoilerVariantConfig>();
        config.name = "Test Spoiler Variant Config";
        config.Overrides = overrides;
        config.FallbackConfig = baseConfig;
        config.OverrideBase = forceUseFallback;
        return config;
    }

    CarFrameBaseConfig createCarFrameBaseConfig()
    {
        var config = ScriptableObject.CreateInstance<CarFrameBaseConfig>();
        config.name = "Test Car Frame Base Config";
        config.FrameColor = Color.white;
        config.WindshieldColor = Color.blue;
        return config;
    }

    CarFrameVariantConfig createCarFrameVariantConfig(CarFrameVariantOverrides overrides, CarFrameBaseConfig baseConfig = null, bool overrideBase = false)
    {
        var config = ScriptableObject.CreateInstance<CarFrameVariantConfig>();
        config.CarFrameVariantOverrides = overrides;
        config.OverrideBase = overrideBase;
        config.BaseConfig = baseConfig;
        return config;
    }

    PartSlotBaseConfig createBasePartSlot(PartSlotType slotType, IBaseConfig config)
    {
        PartSlotBaseConfig result;
        PartSlotData partData = new PartSlotData { LocalPosition = Vector2.up,
            LocalRotation = Vector3.zero, LocalScale = Vector3.one, Hidden = false };
        switch (slotType)
        {
            case PartSlotType.Engine:
                var ec = new EngineSlotBaseConfig();
                ec.engineBaseConfig = config as EngineBaseConfig;
                result = ec;
                partData.Hidden = true;
                partData.Required = true;
                break;
            case PartSlotType.Wheels:
                var wc = new WheelSlotBaseConfig();
                wc.wheelBaseConfig = config as WheelBaseConfig;
                result = wc;
                partData.Required = true;
                break;
            case PartSlotType.Spoiler:
                var sc = new SpoilerSlotBaseConfig();
                sc.spoilerBaseConfig = config as SpoilerBaseConfig;
                partData.Required = false;
                result = sc;
                break;
            default:
                throw new System.Exception("Not supported slot type " + slotType);
        }
        result.partSlotData = partData;
        return result;
    }

    PartSlotVariantConfig createVariantSlot(PartSlotType slotType, IVariantConfig config)
    {
        PartSlotVariantConfig result;
        switch (slotType)
        {
            case PartSlotType.Engine:
                var ec = new EngineSlotVariantConfig();
                ec.engineVariantConfig = config as EngineVariantConfig;
                result = ec;
                break;
            case PartSlotType.Wheels:
                var wc = new WheelSlotVariantConfig();
                wc.wheelVariantConfig = config as WheelVariantConfig;
                result = wc;
                break;
            case PartSlotType.Spoiler:
                var sc = new SpoilerSlotVariantConfig();
                sc.spoilerVariantConfig = config as SpoilerVariantConfig;
                result = sc;
                break;
            default:
                throw new System.Exception("Not supported slot type " + slotType);
        }
        return result;
    }

    CarBaseConfig createCarBaseConfig(CarFrameBaseConfig carFrameBaseConfig, List<PartSlotBaseConfig> slots)
    {
        var config = ScriptableObject.CreateInstance<CarBaseConfig>();
        config.name = "Test Car Base Config";
        config.SlotConfigs = slots;
        config.CarFrameRuntimeConfig = carFrameBaseConfig;
        return config;
    }

    CarVariantConfig createCarVariantConfig(CarFrameVariantConfig carFrameVariantConfig, List<PartSlotVariantConfig> slots)
    {
        var config = ScriptableObject.CreateInstance<CarVariantConfig>();
        config.carFrameRuntimeConfig= carFrameVariantConfig;
        config.slotConfigs= slots;
        return config;
    }
    #endregion
}
