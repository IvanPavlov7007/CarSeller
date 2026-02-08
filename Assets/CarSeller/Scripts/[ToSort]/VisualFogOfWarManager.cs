using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pixelplacement;

public class VisualFogOfWarManager : Singleton<VisualFogOfWarManager>
{
    [Header("Mask Prefab")]
    [Tooltip("Prefab with a SpriteMask (and optionally a SpriteRenderer for debug).")]
    public GameObject CircleMaskPrefab;

    [Header("Runtime")]
    [Tooltip("Optional parent for spawned circle masks. If null, masks are parented to this GameObject.")]
    [SerializeField] private Transform masksParent;

    // Active centers from current game state (city entity -> config)
    private readonly Dictionary<CityEntity, VisionConfig> _activeCenters = new();

    // Vision centers (entity -> mask GO)
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
        GameEvents.Instance.OnGameStateChanged += OnGameStateChanged;
        GameEvents.Instance.OnLocatableDestroyed += OnLocatableDestroyed;
        GameEvents.Instance.OnLocatableLocationChanged += OnLocatableLocationChanged;

        RebuildFromGameState();
    }

    private void OnDisable()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameEvents.Instance.OnLocatableDestroyed -= OnLocatableDestroyed;
            GameEvents.Instance.OnLocatableLocationChanged -= OnLocatableLocationChanged;
        }

        ClearAllMasks();
    }

    private void LateUpdate()
    {
        UpdateMasksAndCenters();
    }

    private void OnGameStateChanged(GameStateChangeEventData data) => RebuildFromGameState();
    private void OnLocatableDestroyed(LocatableDestroyedEventData data) => RebuildFromGameState();
    private void OnLocatableLocationChanged(LocatableLocationChangedEventData data) => RebuildFromGameState();

    public bool TryGetNearestCenter(Vector2 worldPos, out VisionCenterRuntime nearest)
    {
        UpdateMasksAndCenters();

        if (_visionCenters.Count == 0)
        {
            nearest = default;
            return false;
        }

        float best = float.PositiveInfinity;
        VisionCenterRuntime bestCenter = default;

        for (int i = 0; i < _visionCenters.Count; i++)
        {
            var c = _visionCenters[i];
            float d2 = (c.Position - worldPos).sqrMagnitude;
            if (d2 < best)
            {
                best = d2;
                bestCenter = c;
            }
        }

        nearest = bestCenter;
        return true;
    }

    private void RebuildFromGameState()
    {
        _activeCenters.Clear();

        if (G.City == null || G.GameState == null)
        {
            ClearAllMasks();
            return;
        }

        // For now: ONLY focused car or player figure are vision centers.
        AddCenterFor(G.GameState.FocusedCar);
        AddCenterFor(G.GameState.PlayerFigure);

        // Remove masks not needed.
        var toRemove = _circleMasks.Keys.Where(e => !_activeCenters.ContainsKey(e)).ToList();
        for (int i = 0; i < toRemove.Count; i++)
            RemoveMask(toRemove[i]);

        // Add/ensure masks.
        foreach (var kv in _activeCenters)
            EnsureMask(kv.Key);

        UpdateMasksAndCenters();
    }

    private void AddCenterFor(ILocatable locatable)
    {
        if (locatable == null) return;
        if (!G.City.TryGetEntity(locatable, out var entity) || entity == null) return;

        var cfg = ResolveVisionConfig(entity);
        if (cfg == null) return;

        _activeCenters[entity] = cfg;
    }

    private static VisionConfig ResolveVisionConfig(CityEntity entity)
    {
        var center = entity.Aspects?.OfType<VisionCenterAspect>().FirstOrDefault();
        if (center != null && center.Config != null)
            return center.Config;

        // Default if no aspect is attached.
        return new VisionConfig { Radius = 4f, VisionMin = 3f, VisionMax = 4f, ScaleAtMin = 1f, ScaleAtMax = 0.2f, HideBeyondMax = true };
    }

    private void EnsureMask(CityEntity entity)
    {
        if (entity == null) return;
        if (_circleMasks.ContainsKey(entity)) return;

        if (CircleMaskPrefab == null)
        {
            Debug.LogWarning("VisualFogOfWarManager: CircleMaskPrefab is not assigned.");
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
        _activeCenters.Clear();
        _visionCenters.Clear();
    }

    private void UpdateMasksAndCenters()
    {
        _visionCenters.Clear();

        foreach (var kv in _activeCenters)
        {
            var entity = kv.Key;
            var cfg = kv.Value;
            if (entity == null || cfg == null) continue;

            var pos = entity.Position.WorldPosition;

            _visionCenters.Add(new VisionCenterRuntime
            {
                Position = pos,
                Config = cfg
            });

            if (_circleMasks.TryGetValue(entity, out var go) && go != null)
            {
                go.transform.position = new Vector3(pos.x, pos.y, go.transform.position.z);

                float diameter = Mathf.Max(0.01f, cfg.Radius * 2f);
                go.transform.localScale = new Vector3(diameter, diameter, 1f);

                var mask = go.GetComponent<SpriteMask>();
                if (mask != null) mask.isCustomRangeActive = false;
            }
        }
    }
}
