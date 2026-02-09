using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Bridges CityEntity aspects to view representation.
/// Listens to `G.CityEntityAspectsService` and applies/removes view-side components.
/// 
/// This is intentionally lightweight and can be used by `CityViewObjectBuilder` for initial setup.
/// </summary>
public sealed class AspectsViewBuilder
{
    private readonly Dictionary<CityEntity, CityViewObjectController> _views;

    public AspectsViewBuilder(Dictionary<CityEntity, CityViewObjectController> views)
    {
        _views = views ?? throw new ArgumentNullException(nameof(views));

        // Subscribe once.
        G.CityEntityAspectsService.OnAspectAdded -= OnAspectAdded;
        G.CityEntityAspectsService.OnAspectRemoved -= OnAspectRemoved;
        G.CityEntityAspectsService.OnAspectAdded += OnAspectAdded;
        G.CityEntityAspectsService.OnAspectRemoved += OnAspectRemoved;
    }

    public void ApplyAllExistingAspects(CityEntity entity, CityViewObjectController view)
    {
        if (entity == null || view == null) return;

        foreach (var a in entity.Aspects)
            ApplyAspect(view.gameObject, view, entity, a);

        // Post-pass: attach derived components that depend on presence of aspects.
        EnsureDerivedComponents(view.gameObject, entity);
    }

    private void OnAspectAdded(CityEntityAspectAddedEventData e)
    {
        if (e?.Entity == null) return;
        if (!_views.TryGetValue(e.Entity, out var view) || view == null) return;

        ApplyAspect(view.gameObject, view, e.Entity, e.Aspect);
        EnsureDerivedComponents(view.gameObject, e.Entity);
    }

    private void OnAspectRemoved(CityEntityAspectRemovedEventData e)
    {
        if (e?.Entity == null) return;
        if (!_views.TryGetValue(e.Entity, out var view) || view == null) return;

        RemoveAspectView(view.gameObject, view, e.Entity, e.Aspect);
        EnsureDerivedComponents(view.gameObject, e.Entity);
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

            case PinStyleAspect:
                // Pin UI objects are created via CityUIBuilder, but we want to ensure they react to vision as well.
                // The VisionPinScaler component is attached on the pin instance (CityUIPin.Initialize),
                // so here we just ensure derived requirements will be satisfied.
                break;
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

        // If entity has pins and is vision-scaled, ensure its pin has scaler support by adding a CanvasGroup
        // (VisionPinScaler will drive alpha if present).
        bool hasPins = entity.Aspects != null && entity.Aspects.OfType<PinStyleAspect>().Any();
        if (hasPins && needsDistanceScale)
        {
            var pins = UnityEngine.Object.FindObjectsByType<CityUIPin>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < pins.Length; i++)
            {
                var pin = pins[i];
                if (pin == null) continue;

                // Match by view controller reference
                var provider = pin.GetComponentInParent<CityUIPin>();
                if (provider == null) continue;

                // CityUIPin already caches controller internally; we cannot access it, so match by target transform.
                var pos = pin.GetComponent<CityUIPinPositioner>();
                if (pos == null || pos.target != go.transform) continue;

                if (pin.GetComponent<VisionPinScaler>() == null)
                    pin.gameObject.AddComponent<VisionPinScaler>();

                if (pin.GetComponent<CanvasGroup>() == null)
                    pin.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }
}
