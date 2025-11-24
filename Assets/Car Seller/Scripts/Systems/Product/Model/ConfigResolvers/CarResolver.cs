using System;
using System.Collections.Generic;
using System.Linq;

public class CarResolver
{
    private readonly GenericConfigResolver _resolver = new GenericConfigResolver();

    public CarRuntimeConfig Resolve(CarBaseConfig carBase, CarVariantConfig carVariant)
    {
        var result = new CarRuntimeConfig
        {
            Name = carBase.Name,
            CarFrameRuntimeConfig = ResolveFrame(carBase, carVariant),
            SlotConfigs = new List<PartSlotRuntimeConfig>()
        };

        // Build pools
        var basePool = carBase.SlotConfigs != null
            ? new List<PartSlotBaseConfig>(carBase.SlotConfigs)
            : new List<PartSlotBaseConfig>();

        var variantPool = (carVariant != null && carVariant.slotConfigs != null)
            ? new List<PartSlotVariantConfig>(carVariant.slotConfigs)
            : new List<PartSlotVariantConfig>();

        // Compare one-by-one: for each variant slot, try to find a matching base slot by type.
        foreach (var vSlot in variantPool)
        {
            var baseIndex = basePool.FindIndex(b => b.SlotType == vSlot.SlotType);
            if (baseIndex >= 0)
            {
                var bSlot = basePool[baseIndex];
                var runtimeSlot = ResolveSlot(bSlot, vSlot);
                result.SlotConfigs.Add(runtimeSlot);

                // Remove matched base slot from the pool
                basePool.RemoveAt(baseIndex);
            }
        }

        // No variants left: apply remaining base slots as-is
        foreach (var remainingBase in basePool)
        {
            var runtimeSlot = ResolveSlot(remainingBase, null);
            result.SlotConfigs.Add(runtimeSlot);
        }

        return result;
    }

    private CarFrameRuntimeConfig ResolveFrame(CarBaseConfig baseCfg, CarVariantConfig variantCfg)
    {
        return _resolver.Resolve<CarFrameBaseConfig, CarFrameVariantConfig, CarFrameRuntimeConfig>(
            baseCfg.CarFrameRuntimeConfig,
            variantCfg?.carFrameRuntimeConfig
        );
    }

    private PartSlotRuntimeConfig ResolveSlot(PartSlotBaseConfig baseSlot, PartSlotVariantConfig variantSlot)
    {
        switch (baseSlot)
        {
            case WheelSlotBaseConfig wheelBase:
                return ResolveWheelSlot(
                    wheelBase,
                    variantSlot as WheelSlotVariantConfig
                );

            // add EngineSlot, BatterySlot, etc here…

            default:
                throw new Exception($"Unknown slot type {baseSlot}");
        }
    }

    private WheelSlotRuntimeConfig ResolveWheelSlot(
        WheelSlotBaseConfig baseCfg,
        WheelSlotVariantConfig variantCfg)
    {
        var runtime = new WheelSlotRuntimeConfig();

        runtime.wheelConfig = _resolver.Resolve<
            WheelBaseConfig,
            WheelVariantConfig,
            WheelRuntimeConfig
        >(
            baseCfg.wheelBaseConfig,
            variantCfg != null ? variantCfg.wheelVariantConfig : null
        );

        return runtime;
    }
}
