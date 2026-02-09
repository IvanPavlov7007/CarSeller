using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Central place for runtime aspect mutations.
/// This is the only supported way to add/remove aspects after a `CityEntity` is created.
/// Emits events so view and systems can react.
/// </summary>
public sealed class CityEntityAspectsService
{
    public event Action<CityEntityAspectAddedEventData> OnAspectAdded;
    public event Action<CityEntityAspectRemovedEventData> OnAspectRemoved;

    public bool HasAspect<T>(CityEntity entity) where T : CityEntityAspect
        => entity != null && entity.Aspects != null && entity.Aspects.OfType<T>().Any();

    public bool TryAddAspect(CityEntity entity, CityEntityAspect aspect, bool allowDuplicateType = false)
    {
        if (entity == null || aspect == null) return false;

        if (!allowDuplicateType)
        {
            var type = aspect.GetType();
            if (entity.Aspects.Any(a => a != null && a.GetType() == type))
                return false;
        }

        if (!entity.TryAddAspectInternal(aspect))
            return false;

        if (aspect is CityEntityAspectBase b)
            b.Entity = entity;

        OnAspectAdded?.Invoke(new CityEntityAspectAddedEventData(entity, aspect));
        return true;
    }

    public bool TryRemoveAspect<T>(CityEntity entity) where T : CityEntityAspect
    {
        if (entity == null) return false;

        var aspect = entity.Aspects.OfType<T>().FirstOrDefault();
        if (aspect == null) return false;

        if (!entity.TryRemoveAspectInternal(aspect))
            return false;

        if (aspect is CityEntityAspectBase b)
            b.Entity = null;

        OnAspectRemoved?.Invoke(new CityEntityAspectRemovedEventData(entity, aspect));
        return true;
    }

    public bool TryRemoveAspect(CityEntity entity, CityEntityAspect aspect)
    {
        if (entity == null || aspect == null) return false;

        if (!entity.TryRemoveAspectInternal(aspect))
            return false;

        if (aspect is CityEntityAspectBase b)
            b.Entity = null;

        OnAspectRemoved?.Invoke(new CityEntityAspectRemovedEventData(entity, aspect));
        return true;
    }
}
