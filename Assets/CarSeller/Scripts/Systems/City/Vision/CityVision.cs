using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Permanent reactive service that decides which city entities are vision centers.
/// 
/// Rules:
/// - Controlled actor (PlayerFigure if present, else FocusedCar) is a vision center.
/// - Additionally, if ownership changes and the new owner is a Warehouse, the owned item becomes a vision center.
/// 
/// This service performs no "rebuild" scans; it updates incrementally from events.
/// Must be (re)initialized after `GameEvents.Instance.Reset()`.
/// </summary>
public sealed class CityVision
{
    private readonly CityEntityAspectsService _aspects;

    // Current controlled center (vehicle/figure)
    private ILocatable _currentActor;
    private CityEntity _currentActorEntity;

    // Additional vision centers granted by other rules (e.g., ownership)
    private readonly Dictionary<ILocatable, CityEntity> _ownedCenterEntities = new();

    private readonly HashSet<CityEntity> _centers = new();
    public IReadOnlyCollection<CityEntity> Centers => _centers;

    public event Action<CityEntity> OnCenterAdded;
    public event Action<CityEntity> OnCenterRemoved;

    // Default configs (kept internal; can later be moved to config asset)
    private readonly VisionConfig _defaultActorVision = new VisionConfig { Radius = 4f, VisionMin = 3f, VisionMax = 4f, ScaleAtMin = 1f, ScaleAtMax = 0.2f, HideBeyondMax = true };
    private readonly VisionConfig _defaultWarehouseVision = new VisionConfig { Radius = 4f, VisionMin = 3f, VisionMax = 4f, ScaleAtMin = 1f, ScaleAtMax = 0.2f, HideBeyondMax = true };


    public CityVision(CityEntityAspectsService aspects)
    {
        _aspects = aspects;
    }

    public void Initialize()
    {

        Debug.Assert(GameEvents.Instance != null, "CityVision.Initialize: GameEvents.Instance is null");
        Debug.Assert(_aspects != null, "CityVision.Initialize: CityEntityAspectsService is null");

        // React to global game/domain changes.
        GameEvents.Instance.OnGameStateChanged += OnGameStateChanged;
        GameEvents.Instance.OnLocatableDestroyed += OnLocatableDestroyed;
        GameEvents.Instance.OnLocatableLocationChanged += OnLocatableLocationChanged;
        GameEvents.Instance.OnOwnershipChanged += OnOwnershipChanged;

        // Track direct aspect mutations as source of truth for centers set.
        _aspects.SubscribeAdded<VisionCenterAspect>(OnVisionCenterAspectAdded);
        _aspects.SubscribeRemoved<VisionCenterAspect>(OnVisionCenterAspectRemoved);

        // Apply initial state once.
        ApplyActorFromState(G.GameState);
    }

    public void Shutdown()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameEvents.Instance.OnLocatableDestroyed -= OnLocatableDestroyed;
            GameEvents.Instance.OnLocatableLocationChanged -= OnLocatableLocationChanged;
            GameEvents.Instance.OnOwnershipChanged -= OnOwnershipChanged;
        }

        if (_aspects != null)
        {
            _aspects.UnsubscribeAdded<VisionCenterAspect>(OnVisionCenterAspectAdded);
            _aspects.UnsubscribeRemoved<VisionCenterAspect>(OnVisionCenterAspectRemoved);
        }

        // Do not automatically remove aspects on shutdown; keep state as-is.
        _currentActor = null;
        _currentActorEntity = null;
        _ownedCenterEntities.Clear();
        _centers.Clear();
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

    private void OnVisionCenterAspectAdded(CityEntity entity, VisionCenterAspect aspect)
    {
        if (entity == null) return;
        if (_centers.Add(entity))
            OnCenterAdded?.Invoke(entity);
    }

    private void OnVisionCenterAspectRemoved(CityEntity entity, VisionCenterAspect aspect)
    {
        if (entity == null) return;
        if (_centers.Remove(entity))
            OnCenterRemoved?.Invoke(entity);
    }

    private void OnGameStateChanged(GameStateChangeEventData data)
    {
        ApplyActorFromState(data?.newState);
    }

    private void OnLocatableDestroyed(LocatableDestroyedEventData data)
    {
        var loc = data?.Locatable;
        if (loc == null) return;

        if (_currentActor == loc)
            ClearActorCenter();

        if (_ownedCenterEntities.TryGetValue(loc, out var entity))
        {
            _ownedCenterEntities.Remove(loc);
            if (entity != null)
                entity.TryRemoveAspect<VisionCenterAspect>();
        }
    }

    private void OnLocatableLocationChanged(LocatableLocationChangedEventData data)
    {
        // If the controlled actor moved out of city / lost its entity, re-resolve.
        if (data?.Locatable != null && data.Locatable == _currentActor)
        {
            ApplyActorFromState(G.GameState);
        }

        // If an owned-center item moved out of city, remove its center aspect.
        if (data?.Locatable != null && _ownedCenterEntities.TryGetValue(data.Locatable, out var ownedEntity))
        {
            if (data.NewLocation == null || data.NewLocation.Holder != G.City)
            {
                _ownedCenterEntities.Remove(data.Locatable);
                if (ownedEntity != null)
                    ownedEntity.TryRemoveAspect<VisionCenterAspect>();
            }
        }
    }

    private void OnOwnershipChanged(OwnershipChangedEventData data)
    {
        if (data == null) return;

        // Only care about city-locatable items.
        if (data.Item is not ILocatable locatable)
            return;
        Debug.Log($"CityVision: Ownership changed for {locatable}, new owner: {data.NewOwner}");
        if (data.NewOwner == G.Player)
        {
            TryAddOwnedWarehouseCenter(locatable);
        }
        else if (_ownedCenterEntities.TryGetValue(locatable, out var entity))
        {
            // If it was previously granted via warehouse ownership but ownership changed away -> remove.
            _ownedCenterEntities.Remove(locatable);
            if (entity != null)
                entity.TryRemoveAspect<VisionCenterAspect>();
        }
    }

    private void TryAddOwnedWarehouseCenter(ILocatable locatable)
    {
        if (locatable == null) return;
        if (_ownedCenterEntities.ContainsKey(locatable)) return;

        if (G.City == null)
        {
            Debug.LogWarning("CityVision: G.City is null; cannot add warehouse-owned center");
            return;
        }

        if (!G.City.TryGetEntity(locatable, out var entity) || entity == null)
        {
            Debug.LogWarning($"CityVision: Owned locatable {locatable} has no CityEntity (not in city?)");
            return;
        }

        if (entity.Aspects.OfType<VisionCenterAspect>().Any())
        {
            _ownedCenterEntities[locatable] = entity;
            return;
        }

        bool ok = entity.TryAddAspect(new VisionCenterAspect(_defaultWarehouseVision));
        if (!ok)
        {
            Debug.LogWarning($"CityVision: Failed to add VisionCenterAspect for warehouse-owned {locatable}");
            return;
        }

        _ownedCenterEntities[locatable] = entity;
    }

    private void ApplyActorFromState(GameState state)
    {
        if (G.City == null)
        {
            ClearActorCenter();
            return;
        }

        ILocatable nextActor = state?.PlayerFigure != null
            ? (ILocatable)state.PlayerFigure
            : state?.FocusedCar;

        if (ReferenceEquals(nextActor, _currentActor))
            return;

        // Remove prior center if any.
        ClearActorCenter();

        if (nextActor == null)
            return;

        if (!G.City.TryGetEntity(nextActor, out var entity) || entity == null)
        {
            Debug.LogWarning($"CityVision: Controlled actor {nextActor} has no CityEntity; cannot assign VisionCenterAspect");
            _currentActor = nextActor;
            _currentActorEntity = null;
            return;
        }

        // Add new center.
        bool ok = entity.TryAddAspect(new VisionCenterAspect(_defaultActorVision));
        if (!ok)
        {
            Debug.LogWarning($"CityVision: Failed to add VisionCenterAspect on controlled actor {entity.Subject}");
            _currentActor = nextActor;
            _currentActorEntity = entity;
            return;
        }

        _currentActor = nextActor;
        _currentActorEntity = entity;
    }

    private void ClearActorCenter()
    {
        if (_currentActorEntity != null)
        {
            _currentActorEntity.TryRemoveAspect<VisionCenterAspect>();
        }

        _currentActor = null;
        _currentActorEntity = null;
    }
}
