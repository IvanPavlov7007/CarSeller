using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LocationService
{
    public bool MoveProductSilently(Product product, IProductLocation newLocation)
    {
        var currentLocation = GetProductLocation(product);

        if (newLocation.Attach(product))
        {
            if (currentLocation != null)
            {
                currentLocation.Detach();
            }
            World.Instance.productLocations[product] = newLocation;

            return true;
        }

        return false;
    }

    public bool MoveProduct(Product product, IProductLocation newLocation)
    {
        var previousLocation = GetProductLocation(product);
        bool moved = MoveProductSilently(product, newLocation);
        if (moved)
        {
            GameEvents.Instance.OnProductLocationChanged?.Invoke(new ProductLocationChangedEventData(product,newLocation, previousLocation));
        }
        return moved;
    }

    public void RemoveProduct(Product product)
    {
        if (World.Instance.productLocations.TryGetValue(product, out IProductLocation location))
        {
            location.Detach();
            World.Instance.productLocations.Remove(product);
        }
    }

    public static IProductLocation GetProductLocation(Product product)
    {
        if (World.Instance.productLocations.TryGetValue(product, out IProductLocation location))
        {
            return location;
        }
        return null;
    }
}