using Pixelplacement;
using UnityEngine;

/// <summary>
/// Small MonoBehaviour that connects GameEvents to the permanent `G.CityVision` service.
/// Ensures vision center aspect is updated when game state changes.
/// </summary>
public sealed class CityVisionDriver : Singleton<CityVisionDriver>
{
    [Header("Defaults")]
    [SerializeField] private VisionConfig defaultFocusedCarVision = new VisionConfig { Radius = 4f, VisionMin = 3f, VisionMax = 4f, ScaleAtMin = 1f, ScaleAtMax = 0.2f, HideBeyondMax = true };

    [SerializeField] private VisionConfig defaultPlayerFigureVision = new VisionConfig { Radius = 4f, VisionMin = 3f, VisionMax = 4f, ScaleAtMin = 1f, ScaleAtMax = 0.2f, HideBeyondMax = true };

    private CityEntity _currentCenterEntity;

    private void OnEnable()
    {
        GameEvents.Instance.OnGameStateChanged += OnGameStateChanged;
        GameEvents.Instance.OnLocatableDestroyed += OnLocatableDestroyed;
        GameEvents.Instance.OnLocatableLocationChanged += OnLocatableLocationChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameEvents.Instance.OnLocatableDestroyed -= OnLocatableDestroyed;
            GameEvents.Instance.OnLocatableLocationChanged -= OnLocatableLocationChanged;
        }

        ClearCurrentCenter();
    }

    private void OnGameStateChanged(GameStateChangeEventData data) => Refresh();
    private void OnLocatableDestroyed(LocatableDestroyedEventData data) => Refresh();
    private void OnLocatableLocationChanged(LocatableLocationChangedEventData data) => Refresh();

    private void Refresh()
    {
        if (G.City == null)
        {
            Debug.LogWarning("CityVisionDriver: G.City is null; clearing vision center");
            ClearCurrentCenter();
            G.CityVision.RebuildFromCity(G.City);
            return;
        }

        if (G.GameState == null)
        {
            Debug.LogWarning("CityVisionDriver: G.GameState is null; clearing vision center");
            ClearCurrentCenter();
            G.CityVision.RebuildFromCity(G.City);
            return;
        }

        ILocatable target = G.GameState.PlayerFigure != null
            ? (ILocatable)G.GameState.PlayerFigure
            : G.GameState.FocusedCar;

        if (target == null)
        {
            Debug.LogWarning("CityVisionDriver: No controlled locatable (FocusedCar/PlayerFigure); clearing vision center");
            ClearCurrentCenter();
            G.CityVision.RebuildFromCity(G.City);
            return;
        }

        if (!G.City.TryGetEntity(target, out var entity) || entity == null)
        {
            Debug.LogWarning($"CityVisionDriver: Controlled locatable {target} has no CityEntity; clearing vision center");
            ClearCurrentCenter();
            G.CityVision.RebuildFromCity(G.City);
            return;
        }

        if (_currentCenterEntity == entity)
        {
            G.CityVision.RebuildFromCity(G.City);
            return;
        }

        ClearCurrentCenter();

        var cfg = target is PlayerFigure ? defaultPlayerFigureVision : defaultFocusedCarVision;
        bool added = G.CityEntityAspectsService.TryAddAspect(entity, new VisionCenterAspect(cfg));
        if (!added)
        {
            Debug.LogWarning($"CityVisionDriver: Failed to add VisionCenterAspect to {entity.Subject}");
            G.CityVision.RebuildFromCity(G.City);
            return;
        }

        _currentCenterEntity = entity;
        G.CityVision.RebuildFromCity(G.City);
    }

    private void ClearCurrentCenter()
    {
        if (_currentCenterEntity != null)
        {
            G.CityEntityAspectsService.TryRemoveAspect<VisionCenterAspect>(_currentCenterEntity);
            _currentCenterEntity = null;
        }
    }
}
