using NUnit.Framework;
using System.Collections.Generic;

public class World
{
    public HiddenSpace hiddenSpace = new HiddenSpace();
    public Dictionary<Product, IProductLocation> productLocations = new Dictionary<Product, IProductLocation>();

    public static World Instance = new World();

    public static World Reset()
    {
        Instance = new World();
        return Instance;
    }
}

/// <summary>
/// A place for things that exist in the world, but are not stored in a city/warehouse/etc.
/// </summary>
public class HiddenSpace : IProductsHolder
{
    public List<HiddenSpaceLocation> hiddenLocations = new List<HiddenSpaceLocation>();

    public HiddenSpaceLocation GetEmptyLocation()
    {
        var location = new HiddenSpaceLocation();
        return location;
    }

    public IProductLocation[] GetProducts()
    {
        return hiddenLocations.ToArray();
    }

    public class HiddenSpaceLocation : IProductLocation
    {
        public Product Product { get; private set; }

        public IProductsHolder Holder => World.Instance.hiddenSpace;

        public bool Attach(Product product)
        {
            if (product != null)
                return false;
            Product = product;
            World.Instance.hiddenSpace.hiddenLocations.Add(this);
            return true;
        }

        public void Detach()
        {
            if (Product == null)
                return;
            Product = null;
            World.Instance.hiddenSpace.hiddenLocations.Remove(this);
        }
    }
}