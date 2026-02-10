using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Permanent (non-MonoBehaviour) service that manages which CityEntities are vision centers.
/// 
/// Owns the authoritative list of active vision centers based on `VisionCenterAspect`.
/// </summary>
public sealed class CityVisionService
{
    private readonly CityEntityAspectsService _aspects;

    private readonly HashSet<CityEntity> _centers = new();

    public IReadOnlyCollection<CityEntity> Centers => _centers;

    public CityVisionService(CityEntityAspectsService aspects)
    {
        _aspects = aspects;

        if (_aspects != null)
        {
            _aspects.SubscribeAdded<VisionCenterAspect>(OnVisionCenterAdded);
            _aspects.SubscribeRemoved<VisionCenterAspect>(OnVisionCenterRemoved);
        }
        else
        {
            Debug.LogWarning("CityVisionService: AspectsService is null; vision centers won't be tracked via events.");
        }
    }

    private void OnVisionCenterAdded(CityEntity entity, VisionCenterAspect aspect)
    {
        if (entity == null) return;
        _centers.Add(entity);
    }

    private void OnVisionCenterRemoved(CityEntity entity, VisionCenterAspect aspect)
    {
        if (entity == null) return;
        _centers.Remove(entity);
    }

    public void RebuildFromCity(City city)
    {
        _centers.Clear();
        if (city == null) return;

        foreach (var entity in city.GetEntities().Values)
        {
            if (entity == null) continue;
            if (entity.Aspects != null && entity.Aspects.OfType<VisionCenterAspect>().Any())
                _centers.Add(entity);
        }
    }

    public bool TryGetNearestCenter(Vector2 worldPos, out CityEntity nearestEntity, out VisionCenterAspect nearestCenter)
    {
        nearestEntity = null;
        nearestCenter = null;

        if (_centers.Count == 0) return false;

        float best = float.PositiveInfinity;

        foreach (var e in _centers)
        {
            if (e == null) continue;
            var c = e.Aspects?.OfType<VisionCenterAspect>().FirstOrDefault();
            if (c == null || c.Config == null) continue;

            float d2 = (e.Position.WorldPosition - worldPos).sqrMagnitude;
            if (d2 < best)
            {
                best = d2;
                nearestEntity = e;
                nearestCenter = c;
            }
        }

        return nearestEntity != null;
    }
}
