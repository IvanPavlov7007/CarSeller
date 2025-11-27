using System;
using System.Collections;
using UnityEngine;
public class ProductManager
{
    GenericConfigResolver configResolver = new GenericConfigResolver();
    CarConfigResolver carResolver = new CarConfigResolver();

    ProductBuilder productBuilder = new ProductBuilder();

    public Car CreateCar(CarBaseConfig baseConfig, CarVariantConfig variantConfig, IProductLocation location)
    {
        CarRuntimeConfig runtimeConfig = carResolver.Resolve(baseConfig, variantConfig);
        Car car = productBuilder.BuildCar(runtimeConfig);
        
        attachProductToLocation(car, location);

        G.Instance.LocationService.RegisterProductLocation(car, location);
        foreach(var locations in car.GetNonEmptyProductLocations())
        {
            //not rising events here
            G.Instance.LocationService.RegisterProductLocation(locations.Product, locations);
        }

        GameEvents.Instance.OnProductCreated?.Invoke(new ProductCreatedEventData(car));
        return car;
    }

    public Wheel CreateWheel(WheelBaseConfig baseConfig, WheelVariantConfig variantConfig, IProductLocation location)
    {
        WheelRuntimeConfig runtimeConfig = configResolver.Resolve<WheelBaseConfig,WheelVariantConfig,WheelRuntimeConfig>(baseConfig, variantConfig);
        Wheel wheel = productBuilder.BuildWheel(runtimeConfig) as Wheel;

        attachProductToLocation(wheel, location);

        G.Instance.LocationService.RegisterProductLocation(wheel, location);
        GameEvents.Instance.OnProductCreated?.Invoke(new ProductCreatedEventData(wheel));
        return wheel;
    }

    public Engine CreateEngine(EngineBaseConfig baseConfig, EngineVariantConfig variantConfig, IProductLocation location)
    {
        EngineRuntimeConfig runtimeConfig = configResolver.Resolve<EngineBaseConfig,EngineVariantConfig,EngineRuntimeConfig>(baseConfig, variantConfig);
        Engine engine = productBuilder.BuildEngine(runtimeConfig) as Engine;
        
        attachProductToLocation(engine, location);

        G.Instance.LocationService.RegisterProductLocation(engine, location);
        GameEvents.Instance.OnProductCreated?.Invoke(new ProductCreatedEventData(engine));
        return engine;
    }

    public Spoiler CreateSpoiler(SpoilerBaseConfig baseConfig, SpoilerVariantConfig variantConfig, IProductLocation location)
    {
        SpoilerRuntimeConfig runtimeConfig = configResolver.Resolve<SpoilerBaseConfig,SpoilerVariantConfig,SpoilerRuntimeConfig>(baseConfig, variantConfig);
        Spoiler spoiler = productBuilder.BuildSpoiler(runtimeConfig) as Spoiler;

        attachProductToLocation(spoiler, location);

        G.Instance.LocationService.RegisterProductLocation(spoiler, location);
        GameEvents.Instance.OnProductCreated?.Invoke(new ProductCreatedEventData(spoiler));
        return spoiler;
    }

    /// <summary>
    /// internally attach product to location and log error if failed
    /// </summary>
    /// <param name="product"></param>
    /// <param name="location"></param>
    private void attachProductToLocation(Product product, IProductLocation location)
    {
        if(!location.Attach(product))
        {
            Debug.LogError($"Failed to place product {product.Name} at location {location}");
        }
    }

}

public class ProductCreationService
{
    protected Action<Product> createProductAction;

    //Product productCreated(Product product)
    //{

    //}
}


/// <summary>
/// Removing references to a product and notifying the system that it has been destroyed.
/// </summary>
public class ProductDeletionService
{
    public void DeleteProduct(Product product)
    {
        G.Instance.LocationService.RemoveProduct(product);
        GameEvents.Instance.OnProductDestroyed(new ProductDestroyedEventData(product));
    }
}