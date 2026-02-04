
using System.Collections.Generic;
using UnityEngine;

public class CityEntityLifetimeService
{
    public bool TryCreate(ILocatable subject, CityPosition position, out CityEntity entity, params CityEntityAspect[] aspects)
    {
        Debug.Assert(subject != null);

        if (G.City.TryGetEntity(subject, out entity))
        {
            Debug.LogError($"City-only locatable {subject} is already in city.");
            return false;
        }

        entity = new CityEntity(G.City, subject, position);
        G.City.Entities[subject] = entity;
        GameEvents.Instance.OnLocatableRegistered?.Invoke(new LocatableCreatedEventData(subject, entity));

        return true;
    }

    public bool TryMoveToCity(Product product, CityPosition position, out CityEntity entity, params CityEntityAspect[] aspects)
    {
        Debug.Assert(product != null);

        if (G.City.TryGetEntity(product, out entity))
        {
            Debug.LogError($"City-only locatable {product} is already in city.");
            entity = G.City.Entities[product];
            return false;
        }

        entity = new CityEntity(G.City, position);
        G.City.Entities[product] = entity;
        return G.ProductLifetimeService.MoveProduct(product, entity);
    }

    private readonly HashSet<ILocatable> _destroyInProgress = new HashSet<ILocatable>();
    public void Destroy(ILocatable anyLocatable)
    {
        if (anyLocatable == null)
            return;

        // Prevent double-destroy / re-entrancy (e.g., onBeingDestroyed handler calls Destroy again)
        if (_destroyInProgress.Contains(anyLocatable))
            return;

        _destroyInProgress.Add(anyLocatable);
        try
        {
            // Notify internal destroy trackers no matter who initiated the destruction.
            if (anyLocatable is IDestroyTracker tracker)
            {
                tracker.NotifyDestroyed();
            }

            switch (anyLocatable)
            {
                case Product p:
                    G.ProductLifetimeService.DestroyProduct(p);
                    if (!removeEntity(p))
                        Debug.LogError($"Failed to remove city entity for locatable {anyLocatable}");
                    break;

                default:
                    if (!removeEntity(anyLocatable))
                        Debug.LogError($"Failed to remove city entity for locatable {anyLocatable}");

                    GameEvents.Instance.OnLocatableDestroyed?.Invoke(new LocatableDestroyedEventData(anyLocatable));
                    break;
            }
        }
        finally
        {
            _destroyInProgress.Remove(anyLocatable);
        }
    }

    bool removeEntity(ILocatable locatable)
    {
        Debug.Assert(locatable != null);
        if (locatable == null)
            return false;
        return G.City.Entities.Remove(locatable);
    }
}
