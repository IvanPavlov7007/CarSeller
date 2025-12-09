using Pixelplacement;
using System;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

public class CitySceneManager : Singleton<CitySceneManager>
{
    public City City => World.Instance.City;

    private void Awake()
    {
        initializeCity();
        G.Instance.CityRoot.SetActive(true);
    }
    private void OnEnable()
    {
        GameEvents.Instance.OnProductCreated += onNewProductCreated;
        GameEvents.Instance.OnProductLocationChanged += onProductLocationChanged;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnProductCreated -= onNewProductCreated;
        GameEvents.Instance.OnProductLocationChanged -= onProductLocationChanged;
        if(G.Instance.CityRoot != null)
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
        initializeObjects();
    }

    private void initializeMap()
    {
        G.Instance.cityViewStreetsBuilder.BuildStreets(City);
    }

    private void initializeObjects()
    {
        foreach (var obj in City.Locations.Keys)
        {
            Debug.Assert(obj != null, "City object cannot be null");
            G.Instance.cityViewObjectBuilder.BuildObject(obj);
        }

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

}