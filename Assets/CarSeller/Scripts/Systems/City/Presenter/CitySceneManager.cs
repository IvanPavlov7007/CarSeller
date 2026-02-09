using Pixelplacement;
using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Managers that loads and manages city in the city scene
/// Set ups and changes states( visuals, interactive) of city views based on the game state
/// Creates and destroys views for city entities
/// </summary>
public class CitySceneManager : Singleton<CitySceneManager>
{
    public City City => World.Instance.City;

    CitySceneProfileRegistry profileRegistry = new CitySceneProfileRegistry();
    CitySceneProfile currentProfile;

    Dictionary<CityEntity, CityViewObjectController> builtObjectsViews = new Dictionary<CityEntity, CityViewObjectController>();

    private AspectsViewBuilder _aspectsViewBuilder;

    public void SetCurrentProfile(GameState state)
    {
        currentProfile = profileRegistry.Get(state);
    }


    private void OnEnable()
    {
        GameEvents.Instance.OnLocatableRegistered += onNewLocatableCreated;
        GameEvents.Instance.OnLocatableLocationChanged += onLocatableLocationChanged;
        GameEvents.Instance.OnLocatableDestroyed += onLocatableDestroyed;
        GameEvents.Instance.OnOwnershipChanged += onOwnershipChanged;

        GameEvents.Instance.OnGameStateChanged += onGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnLocatableRegistered -= onNewLocatableCreated;
        GameEvents.Instance.OnLocatableLocationChanged -= onLocatableLocationChanged;
        GameEvents.Instance.OnLocatableDestroyed -= onLocatableDestroyed;
        GameEvents.Instance.OnOwnershipChanged -= onOwnershipChanged;

        GameEvents.Instance.OnGameStateChanged -= onGameStateChanged;
    }

    public void InitializeCity()
    {
        if (City == null)
        {
            Debug.LogWarning("City instance is not set");
            return;
        }
        initializeMap();
        rebuildSceneForState(G.GameState);
    }

    private void initializeMap()
    {
        G.cityViewStreetsBuilder.BuildStreets(City);
    }

    private void registerNewView(CityEntity entity, CityViewObjectController view)
    {
        builtObjectsViews[entity] = view;

        // Lazy init so we have the views map.
        _aspectsViewBuilder ??= new AspectsViewBuilder(builtObjectsViews);
        _aspectsViewBuilder.ApplyAllExistingAspects(entity, view);
    }

    private void clearView(CityEntity entity)
    {
        if (builtObjectsViews.TryGetValue(entity, out var view))
        {
            Destroy(view.gameObject);
            builtObjectsViews.Remove(entity);
        }
    }

    private void clearViews()
    {
        foreach (var view in builtObjectsViews.Values)
        {
            Destroy(view.gameObject);
        }
        builtObjectsViews.Clear();
    }

    private void onLocatableLocationChanged(LocatableLocationChangedEventData data)
    {
        Debug.Assert(data.Locatable != null, "Locatable cannot be null in location change event data");
        Debug.Assert(data.NewLocation != null, "New location cannot be null in location change event data");
        Debug.Assert(data.OldLocation != null, "Old location cannot be null in location change event data");


        if (data.OldLocation.Holder == City)
        {
            if (G.City.TryGetEntity(data.Locatable, out var entity))
            {
                clearView(entity);
            }

        }

        if (data.NewLocation.Holder == City)
        {
            if (G.City.TryGetEntity(data.Locatable, out var entity))
            {
                applyProfileToObject(entity);
            }
        }
    }

    private void onNewLocatableCreated(LocatableCreatedEventData data)
    {
        if (data.Location.Holder == City)
        {
            if (G.City.TryGetEntity(data.Locatable, out var entity))
            {
                applyProfileToObject(entity);
            }
        }
    }

    private void onLocatableDestroyed(LocatableDestroyedEventData data)
    {
        if (G.City.TryGetEntity(data.Locatable, out var entity))
        {
            clearView(entity);
        }
    }

    private void onGameStateChanged(GameStateChangeEventData data)
    {
        Debug.Assert(data.newState != null, "New game state cannot be null");
        currentProfile?.OnProfileDeactivated();
        currentProfile = profileRegistry.Get(data.newState);
        currentProfile.OnProfileActivated(data.newState);
        rebuildSceneForState(data.newState);
    }

    private void onOwnershipChanged(OwnershipChangedEventData data)
    {
        if (data.Item is not ILocatable locatable)
            return;

        if ( City.TryGetEntity(locatable, out var entity))
        {
            applyProfileToObject(entity);
        }
    }

    private void rebuildSceneForState(GameState state)
    {
        // rebuild views based on the current profile
        foreach (var entity in City.GetEntities().Values)
        {
            applyProfileToObject(entity, state);
        }
    }

    private void applyProfileToObject(CityEntity entity, GameState state)
    {
        Debug.Assert(currentProfile != null);
        Debug.Assert(state != null);
        Debug.Assert(entity != null);

        // if rebuilding only partial views, check if the view exists
        if (!currentProfile.ShouldShow(entity, state))
        {
            clearView(entity);
            return;
        }

        var visualState = currentProfile.GetObjectViewState(entity, state);

        if (builtObjectsViews.TryGetValue(entity, out var existingView))
        {
            // update existing view
            existingView.SetViewState(visualState);
            return;
        }
        else
        {

            CityViewObjectController view = G.cityViewObjectBuilder.BuildObject(entity);
            view.SetViewState(visualState);
            registerNewView(entity, view);
        }
    }

    private void applyProfileToObject(CityEntity entity)
    {
        applyProfileToObject(entity, G.GameState);
    }
}

public sealed class CitySceneProfileRegistry
{
    private readonly Dictionary<Type, CitySceneProfile> _profiles;

    public CitySceneProfileRegistry()
    {
        _profiles = new()
        {
            { typeof(NeutralGameState), new NormalCitySceneProfile() },
            { typeof(StealingGameState), new StealingCitySceneProfile() },
            { typeof(SellingGameState), new SellingCitySceneProfile() },
            { typeof(FreeRoamGameState), new FreeRoamCitySceneProfile() },
            { typeof(MissionGameState), new MissionCitySceneProfile() }
        };
    }

    public CitySceneProfile Get(GameState state)
        => _profiles[state.GetType()];
}
