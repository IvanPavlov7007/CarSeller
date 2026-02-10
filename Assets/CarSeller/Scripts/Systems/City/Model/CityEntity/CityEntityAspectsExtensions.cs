using System;

public static class CityEntityAspectsExtensions
{
    public static bool TryAddAspect<TAspect>(this CityEntity entity, TAspect aspect, bool allowDuplicateType = false)
        where TAspect : class, CityEntityAspect
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (aspect == null) throw new ArgumentNullException(nameof(aspect));
        if (G.CityEntityAspectsService == null) throw new InvalidOperationException("G.CityEntityAspectsService is not initialized");

        return G.CityEntityAspectsService.TryAddAspect(entity, aspect, allowDuplicateType);
    }

    public static bool TryRemoveAspect<TAspect>(this CityEntity entity)
        where TAspect : CityEntityAspect
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (G.CityEntityAspectsService == null) throw new InvalidOperationException("G.CityEntityAspectsService is not initialized");

        return G.CityEntityAspectsService.TryRemoveAspect<TAspect>(entity);
    }
}
