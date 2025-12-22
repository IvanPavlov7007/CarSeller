using Pixelplacement;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CitySceneManager : Singleton<CitySceneManager>
{
    public City City => World.Instance.City;

    CitySceneProfileRegistry profileRegistry = new CitySceneProfileRegistry();
    CitySceneProfile currentProfile;

    Dictionary<ILocatable, CityViewObjectController> builtObjectsViews = new Dictionary<ILocatable, CityViewObjectController>();

    private void Awake()
    {
        initializeCity();
        G.Instance.CityRoot.SetActive(true);
    }
    private void OnEnable()
    {
        GameEvents.Instance.OnLocatableCreated += onNewLocatableCreated;
        GameEvents.Instance.OnLocatableLocationChanged += onLocatableLocationChanged;
        GameEvents.Instance.OnLocatableDestroyed += onLocatableDestroyed;
        GameEvents.Instance.OnLocatableStateChanged += onLocatableStateChanged;

        GameEvents.Instance.OnGameStateChanged += onGameStateChanged;

        currentProfile = profileRegistry.Get(G.GameState);
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnLocatableCreated -= onNewLocatableCreated;
        GameEvents.Instance.OnLocatableLocationChanged -= onLocatableLocationChanged;
        GameEvents.Instance.OnLocatableDestroyed -= onLocatableDestroyed;
        GameEvents.Instance.OnLocatableStateChanged -= onLocatableStateChanged;

        GameEvents.Instance.OnGameStateChanged -= onGameStateChanged;

        if (G.Instance.CityRoot != null)
            G.Instance.CityRoot.SetActive(false);
    }

    void initializeCity()
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
        G.Instance.cityViewStreetsBuilder.BuildStreets(City);
    }

    private void registerNewView(ILocatable locatable, CityViewObjectController view)
    {
        builtObjectsViews[locatable] = view;
    }

    private void clearView(ILocatable locatable)
    {
        if (builtObjectsViews.TryGetValue(locatable, out var view))
        {
            Destroy(view.gameObject);
            builtObjectsViews.Remove(locatable);
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
        if(data.NewLocation.Holder == City)
        {
            applyProfileToObject(data.Locatable);
        }
    }

    private void onNewLocatableCreated(LocatableCreatedEventData data)
    {
        if (data.Location.Holder == City)
        {
            applyProfileToObject(data.Locatable);
        }
    }

    private void onLocatableDestroyed(LocatableDestroyedEventData data)
    {
        clearView(data.Locatable);
    }

    private void onGameStateChanged(GameStateChangeEventData data)
    {
        Debug.Assert(data.newState != null, "New game state cannot be null");
        currentProfile?.OnProfileDeactivated();
        currentProfile = profileRegistry.Get(data.newState);
        currentProfile.OnProfileActivated(data.newState);
        rebuildSceneForState(data.newState);
    }

    private void onLocatableStateChanged(LocatableStateChangedEventData data)
    {
        if ( CityPositionLocator.IsInCity(data.Locatable))
        {
            applyProfileToObject(data.Locatable);
        }
    }

    private void rebuildSceneForState(GameState state)
    {
        // clear existing views
        clearViews();

        // rebuild views based on the current profile

        foreach (var product in City.Locations.Keys)
        {
            applyProfileToObject(product, state);
        }
    }

    private void applyProfileToObject(ILocatable locatable, GameState state)
    {
        // if rebuilding only partial views, check if the view exists
        if (!currentProfile.ShouldShow(locatable, state))
        {
            clearView(locatable);
            return;
        }

        var visualState = currentProfile.GetObjectViewState(locatable, state);

        if (builtObjectsViews.TryGetValue(locatable, out var existingView))
        {
            // update existing view
            existingView.SetViewState(visualState);
            return;
        }
        else
        {

            CityViewObjectController view = G.Instance.cityViewObjectBuilder.BuildObject(locatable);
            view.SetViewState(visualState);
            registerNewView(locatable, view);
        }
    }

    private void applyProfileToObject(ILocatable locatable)
    {
        applyProfileToObject(locatable, G.GameState);
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
        };
    }

    public CitySceneProfile Get(GameState state)
        => _profiles[state.GetType()];
}
