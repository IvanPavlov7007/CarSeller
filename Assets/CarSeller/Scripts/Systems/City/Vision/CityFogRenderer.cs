using System.Collections.Generic;
using System.Linq;
using Pixelplacement;
using UnityEngine;

/// <summary>
/// Pure rendering layer for fog-of-war.
/// Tracks city entities that currently have `VisionCenterAspect` and maintains SpriteMask circles.
/// 
/// Game logic that decides which entities are centers lives in `CityVision`.
/// </summary>
public sealed class CityFogRenderer : Singleton<CityFogRenderer>
{
    [Header("Mask Prefab")]
    [Tooltip("Prefab with a SpriteMask (and optionally a SpriteRenderer for debug).")]
    public GameObject CircleMaskPrefab;

    [Header("Runtime")]
    [Tooltip("Optional parent for spawned circle masks. If null, masks are parented to this GameObject.")]
    [SerializeField] private Transform masksParent;

    private readonly Dictionary<CityEntity, GameObject> _circleMasks = new();

    public struct VisionCenterRuntime
    {
        public Vector2 Position;
        public VisionConfig Config;
    }

    public IReadOnlyList<VisionCenterRuntime> VisionCenters => _visionCenters;
    private readonly List<VisionCenterRuntime> _visionCenters = new();

    protected override void OnRegistration()
    {
        base.OnRegistration();
        if (masksParent == null) masksParent = transform;
    }

    private void OnEnable()
    {
        // Aspect event-driven updates.
        G.CityEntityAspectsService.OnAspectAdded += OnAspectAdded;
        G.CityEntityAspectsService.OnAspectRemoved += OnAspectRemoved;

        GameEvents.Instance.OnLocatableDestroyed += OnLocatableDestroyed;
        GameEvents.Instance.OnLocatableLocationChanged += OnLocatableLocationChanged;

        // Initial sync.
        G.CityVision.RebuildFromCity(G.City);
        Rebuild();
    }

    private void OnDisable()
    {
        if (G.CityEntityAspectsService != null)
        {
            G.CityEntityAspectsService.OnAspectAdded -= OnAspectAdded;
            G.CityEntityAspectsService.OnAspectRemoved -= OnAspectRemoved;
        }

        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnLocatableDestroyed -= OnLocatableDestroyed;
            GameEvents.Instance.OnLocatableLocationChanged -= OnLocatableLocationChanged;
        }

        ClearAllMasks();
    }

    private void LateUpdate()
    {
        UpdateMasksAndCenters();
    }

    private void OnAspectAdded(CityEntityAspectAddedEventData e)
    {
        if (e?.Entity == null) return;
        if (e.Aspect is VisionCenterAspect) Rebuild();
    }

    private void OnAspectRemoved(CityEntityAspectRemovedEventData e)
    {
        if (e?.Entity == null) return;
        if (e.Aspect is VisionCenterAspect) Rebuild();
    }

    private void OnLocatableDestroyed(LocatableDestroyedEventData data) => Rebuild();
    private void OnLocatableLocationChanged(LocatableLocationChangedEventData data) => Rebuild();

    private void Rebuild()
    {
        if (G.City == null)
        {
            ClearAllMasks();
            return;
        }

        // Use CityVision as the authoritative source.
        var centers = G.CityVision.Centers.Where(e => e != null).ToList();

        // Remove missing.
        var toRemove = _circleMasks.Keys.Where(e => !centers.Contains(e)).ToList();
        for (int i = 0; i < toRemove.Count; i++)
            RemoveMask(toRemove[i]);

        // Ensure existing.
        for (int i = 0; i < centers.Count; i++)
            EnsureMask(centers[i]);

        UpdateMasksAndCenters();
    }

    private void EnsureMask(CityEntity entity)
    {
        if (entity == null) return;
        if (_circleMasks.ContainsKey(entity)) return;

        if (CircleMaskPrefab == null)
        {
            Debug.LogWarning("CityFogRenderer: CircleMaskPrefab is not assigned.");
            return;
        }

        var go = Instantiate(CircleMaskPrefab, masksParent);
        go.name = $"VisionMask_{entity.Subject?.GetType().Name ?? "Entity"}";
        _circleMasks[entity] = go;
    }

    private void RemoveMask(CityEntity entity)
    {
        if (entity == null) return;
        if (_circleMasks.TryGetValue(entity, out var go))
        {
            if (go != null) Destroy(go);
            _circleMasks.Remove(entity);
        }
    }

    private void ClearAllMasks()
    {
        foreach (var kv in _circleMasks)
        {
            if (kv.Value != null) Destroy(kv.Value);
        }
        _circleMasks.Clear();
        _visionCenters.Clear();
    }

    public bool TryGetNearestCenter(Vector2 worldPos, out VisionCenterRuntime nearest)
    {
        // Delegate to CityVision.
        if (!G.CityVision.TryGetNearestCenter(worldPos, out var entity, out var center) || center?.Config == null)
        {
            nearest = default;
            return false;
        }

        nearest = new VisionCenterRuntime
        {
            Position = entity.Position.WorldPosition,
            Config = center.Config
        };
        return true;
    }

    private void UpdateMasksAndCenters()
    {
        _visionCenters.Clear();

        foreach (var kv in _circleMasks)
        {
            var entity = kv.Key;
            var go = kv.Value;
            if (entity == null || go == null) continue;

            var center = entity.Aspects?.OfType<VisionCenterAspect>().FirstOrDefault();
            if (center == null || center.Config == null) continue;

            var pos = entity.Position.WorldPosition;

            _visionCenters.Add(new VisionCenterRuntime
            {
                Position = pos,
                Config = center.Config
            });

            go.transform.position = new Vector3(pos.x, pos.y, go.transform.position.z);
            float diameter = Mathf.Max(0.01f, center.Config.Radius * 2f);
            go.transform.localScale = new Vector3(diameter, diameter, 1f);

            var mask = go.GetComponent<SpriteMask>();
            if (mask != null) mask.isCustomRangeActive = false;
        }
    }
}
