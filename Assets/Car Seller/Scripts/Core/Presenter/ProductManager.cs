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


        //We don't want to rise event here, as we are creating a complex product
        InitializeProductLocation(car, location);
        foreach(var locations in car.GetProducts())
        {
            //not rising events here
            InitializeProductLocation(locations.Product, locations);
        }

        GameEvents.Instance.OnProductCreated?.Invoke(new ProductCreatedEventData(car));
        return car;
    }

    public Wheel CreateWheel(WheelBaseConfig baseConfig, WheelVariantConfig variantConfig, IProductLocation location)
    {
        WheelRuntimeConfig runtimeConfig = configResolver.Resolve<WheelBaseConfig,WheelVariantConfig,WheelRuntimeConfig>(baseConfig, variantConfig);
        Wheel wheel = productBuilder.BuildWheel(runtimeConfig) as Wheel;
        InitializeProductLocation(wheel, location);
        GameEvents.Instance.OnProductCreated?.Invoke(new ProductCreatedEventData(wheel));
        return wheel;
    }

    public Engine CreateEngine(EngineBaseConfig baseConfig, EngineVariantConfig variantConfig, IProductLocation location)
    {
        EngineRuntimeConfig runtimeConfig = configResolver.Resolve<EngineBaseConfig,EngineVariantConfig,EngineRuntimeConfig>(baseConfig, variantConfig);
        Engine engine = productBuilder.BuildEngine(runtimeConfig) as Engine;
        InitializeProductLocation(engine, location);
        GameEvents.Instance.OnProductCreated?.Invoke(new ProductCreatedEventData(engine));
        return engine;
    }

    public Spoiler CreateSpoiler(SpoilerBaseConfig baseConfig, SpoilerVariantConfig variantConfig, IProductLocation location)
    {
        SpoilerRuntimeConfig runtimeConfig = configResolver.Resolve<SpoilerBaseConfig,SpoilerVariantConfig,SpoilerRuntimeConfig>(baseConfig, variantConfig);
        Spoiler spoiler = productBuilder.BuildSpoiler(runtimeConfig) as Spoiler;
        InitializeProductLocation(spoiler, location);
        GameEvents.Instance.OnProductCreated?.Invoke(new ProductCreatedEventData(spoiler));
        return spoiler;
    }


    /// <summary>
    /// silently place product at location without rising events
    /// </summary>
    /// <param name="product"></param>
    /// <param name="location"></param>
    protected void InitializeProductLocation(Product product, IProductLocation location)
    {
        //not rising event here
        if (!G.Instance.LocationService.MoveProductSilently(product, location))
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