using Pixelplacement;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CitySceneManager : Singleton<CitySceneManager>
{
    public City City => World.Instance.City;

    CitySceneProfileRegistry profileRegistry = new CitySceneProfileRegistry();
    CitySceneProfile currentProfile;

    Dictionary<ILocatable, GameObject> builtObjectsViews = new Dictionary<ILocatable, GameObject>();

    private void Awake()
    {
        initializeCity();
        G.Instance.CityRoot.SetActive(true);
    }
    private void OnEnable()
    {
        GameEvents.Instance.OnProductCreated += onNewProductCreated;
        GameEvents.Instance.OnProductLocationChanged += onProductLocationChanged;
        GameEvents.Instance.OnGameStateChanged += onGameStateChanged;
        currentProfile = profileRegistry.Get(G.GameState);
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnProductCreated -= onNewProductCreated;
        GameEvents.Instance.OnProductLocationChanged -= onProductLocationChanged;
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

    private void registerNewView(ILocatable locatable, GameObject view)
    {
        builtObjectsViews[locatable] = view;
    }

    private void clearView(ILocatable locatable)
    {
        if (builtObjectsViews.TryGetValue(locatable, out var view))
        {
            Destroy(view);
            builtObjectsViews.Remove(locatable);
        }
    }

    private void clearViews()
    {
        foreach (var view in builtObjectsViews.Values)
        {
            Destroy(view);
        }
        builtObjectsViews.Clear();
    }

    private void onProductLocationChanged(ProductLocationChangedEventData data)
    {
        if(data.NewLocation.Holder == City)
        {
            G.Instance.cityViewObjectBuilder.BuildObject(data.Product);
        }
    }

    private void onNewProductCreated(ProductCreatedEventData data)
    {
        if (data.Location.Holder == City)
        {
            G.Instance.cityViewObjectBuilder.BuildObject(data.Product);
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
        //if (!currentProfile.ShouldShow(locatable, state))
        //{
        //    clearView(locatable);
        //    return;
        //}

        // if the profile says not to show, skip
        if (!currentProfile.ShouldShow(locatable, state))
        {
            return;
        }

        var visualState = currentProfile.GetObjectVisualState(locatable, state);

        GameObject view = null;

        switch (visualState)
        {
            case CityObjectVisualState.Normal:
                view = G.Instance.cityViewObjectBuilder.BuildObject(locatable);
                break;
            case CityObjectVisualState.Disabled:
                view = G.Instance.cityViewObjectBuilder.BuildObjectDisabled(locatable);
                break;
            default:
                break;
        }
        registerNewView(locatable,view);
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
