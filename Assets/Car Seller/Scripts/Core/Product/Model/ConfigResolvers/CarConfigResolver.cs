using System;
using System.Collections.Generic;
using System.Linq;

public class CarConfigResolver
{
    private readonly GenericConfigResolver _resolver = new GenericConfigResolver();
    private readonly PartSlotResolver _partSlotResolver;

    public CarConfigResolver()
    {
        _partSlotResolver = new PartSlotResolver(_resolver);
    }

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

        var variantPool = (carVariant?.slotConfigs != null)
            ? new List<PartSlotVariantConfig>(carVariant.slotConfigs)
            : new List<PartSlotVariantConfig>();

        // Compare one-by-one: for each variant slot, try to find a matching base slot by type.
        foreach (var vSlot in variantPool)
        {
            var idx = basePool.FindIndex(b => b.SlotType == vSlot.SlotType);
            if (idx >= 0)
            {
                var bSlot = basePool[idx];
                result.SlotConfigs.Add(_partSlotResolver.Resolve(bSlot, vSlot));
                basePool.RemoveAt(idx);
            }
        }

        // No variants left: apply remaining base slots as-is
        foreach (var remaining in basePool)
        {
            result.SlotConfigs.Add(_partSlotResolver.Resolve(remaining, null));
        }

        return result;
    }

    private CarFrameRuntimeConfig ResolveFrame(CarBaseConfig baseCfg, CarVariantConfig variantCfg)
    {
        return _resolver.Resolve<CarFrameBaseConfig, CarFrameVariantConfig, CarFrameRuntimeConfig>(
            baseCfg.CarFrameBaseConfig,
            variantCfg?.carFrameRuntimeConfig
        );
    }
}
