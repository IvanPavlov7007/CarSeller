using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General service to manage product locations. Should be used as the only tool to move products between locations.
/// </summary>
public class LocationService
{
    Dictionary<Product, IProductLocation> productLocations => World.Instance.productLocations;

    public void RegisterProductLocation(Product product, IProductLocation location)
    {
        Debug.Assert(product != null, "Product cannot be null when registering a location.");
        Debug.Assert(location != null, "Location cannot be null when registering a product.");
        Debug.Assert(!productLocations.ContainsKey(product), $"Product {product.Name} is already registered with a location.");

        productLocations[product] = location;
    }

    public bool MoveProduct(Product product, IProductLocation newLocation)
    {
        var previousLocation = GetProductLocation(product);

        if (newLocation.Attach(product))
        {
            if (previousLocation != null)
            {
                previousLocation.Detach();
            }
            productLocations[product] = newLocation;
            GameEvents.Instance.OnProductLocationChanged?.Invoke(new ProductLocationChangedEventData(product, newLocation, previousLocation));
            return true;
        }

        return false;
    }

    public void RemoveProduct(Product product)
    {
        if (productLocations.TryGetValue(product, out IProductLocation location))
        {
            location.Detach();
            productLocations.Remove(product);
            GameEvents.Instance.OnProductLocationChanged?.Invoke(new ProductLocationChangedEventData(product, null, location));
        }
    }

    public IProductLocation GetProductLocation(Product product)
    {
        if (productLocations.TryGetValue(product, out IProductLocation location))
        {
            return location;
        }
        return null;
    }
}