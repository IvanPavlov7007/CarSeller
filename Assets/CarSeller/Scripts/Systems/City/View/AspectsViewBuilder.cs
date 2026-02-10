using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Bridges CityEntity aspects to view representation.
/// Listens to `G.CityEntityAspectsService` and applies/removes view-side components.
/// </summary>
public sealed class AspectsViewBuilder
{
    private readonly Dictionary<CityEntity, CityViewObjectController> _views;

    public AspectsViewBuilder(Dictionary<CityEntity, CityViewObjectController> views)
    {
        _views = views ?? throw new ArgumentNullException(nameof(views));

        Debug.Assert(G.CityEntityAspectsService != null, "AspectsViewBuilder: G.CityEntityAspectsService is null");

        // Subscribe to typed aspect notifications.
        G.CityEntityAspectsService.SubscribeAdded<VisionDistanceScaleAspect>(OnVisionDistanceScaleAdded);
        G.CityEntityAspectsService.SubscribeRemoved<VisionDistanceScaleAspect>(OnVisionDistanceScaleRemoved);

        // If later we need pin reactions, subscribe to PinStyleAspect as well.
    }

    public void ApplyAllExistingAspects(CityEntity entity, CityViewObjectController view)
    {
        if (entity == null || view == null)
        {
            Debug.LogWarning("AspectsViewBuilder.ApplyAllExistingAspects called with nulls");
            return;
        }

        foreach (var a in entity.Aspects)
            ApplyAspect(view.gameObject, view, entity, a);

        EnsureDerivedComponents(view.gameObject, entity);
    }

    // (typed aspect callbacks are registered in the constructor)

    private void OnVisionDistanceScaleAdded(CityEntity entity, VisionDistanceScaleAspect aspect)
    {
        if (entity == null) return;
        if (!_views.TryGetValue(entity, out var view) || view == null) return;

        ApplyAspect(view.gameObject, view, entity, aspect);
        EnsureDerivedComponents(view.gameObject, entity);
    }

    private void OnVisionDistanceScaleRemoved(CityEntity entity, VisionDistanceScaleAspect aspect)
    {
        if (entity == null) return;
        if (!_views.TryGetValue(entity, out var view) || view == null) return;

        RemoveAspectView(view.gameObject, view, entity, aspect);
        EnsureDerivedComponents(view.gameObject, entity);
    }

    private static void ApplyAspect(GameObject go, CityViewObjectController view, CityEntity entity, CityEntityAspect aspect)
    {
        if (go == null || entity == null || aspect == null) return;

        switch (aspect)
        {
            case VisionDistanceScaleAspect:
                if (go.GetComponent<VisionDistanceScaler>() == null)
                    go.AddComponent<VisionDistanceScaler>();
                break;

            // PinStyleAspect visual representation is created by CityViewObjectBuilder.
            // Pin-side Vision scaling is handled by `VisionPinScaler` attached to the pin prefab instance.
        }
    }

    private static void RemoveAspectView(GameObject go, CityViewObjectController view, CityEntity entity, CityEntityAspect aspect)
    {
        if (go == null || entity == null || aspect == null) return;

        switch (aspect)
        {
            case VisionDistanceScaleAspect:
                {
                    var s = go.GetComponent<VisionDistanceScaler>();
                    if (s != null) UnityEngine.Object.Destroy(s);
                    break;
                }
        }
    }

    private static void EnsureDerivedComponents(GameObject go, CityEntity entity)
    {
        if (go == null || entity == null) return;

        bool needsDistanceScale = entity.Aspects != null && entity.Aspects.OfType<VisionDistanceScaleAspect>().Any();
        var scaler = go.GetComponent<VisionDistanceScaler>();

        if (needsDistanceScale)
        {
            if (scaler == null) go.AddComponent<VisionDistanceScaler>();
        }
        else
        {
            if (scaler != null) UnityEngine.Object.Destroy(scaler);
        }

        // No pin searching here.
        // Pins are separate objects; they attach `VisionPinScaler` at creation time in `CityUIPin.Initialize()`.
        // When aspects change dynamically, the correct solution is to have pin instances subscribe to aspect events,
        // or to track pins by CityEntity in a registry (future improvement).
    }
}
