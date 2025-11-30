using Pixelplacement;
using System;
using UnityEngine;

public class CitySceneManager : Singleton<CitySceneManager>
{
    public City City => World.Instance.City;
    private void Start()
    {
        initializeCity();
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
    }

    void initializeCity()
    {
        if (City == null)
        {
            Debug.LogWarning("City instance is not set");
            return;
        }
        foreach (var obj in City.Objects.Keys)
        {
            Debug.Assert(obj != null, "City object cannot be null");

            switch(obj)
            {
                case Car:                    // Initialize car in the scene
                    break;
                case Warehouse:              // Initialize warehouse in the scene
                    break;
                // Add cases for different object types to initialize them in the scene
                default:
                    Debug.LogWarning($"Unhandled city object type: {obj.GetType().Name}");
                    break;
            }   
        }
    }

    private void initializeMap()
    {

    }

    private void initializeObjects()
    {

    }

    private void onProductLocationChanged(ProductLocationChangedEventData data)
    {
        throw new NotImplementedException();
    }

    private void onNewProductCreated(ProductCreatedEventData data)
    {
        throw new NotImplementedException();
    }

}