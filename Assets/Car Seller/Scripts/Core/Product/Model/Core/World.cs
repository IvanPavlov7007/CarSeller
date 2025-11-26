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
public class HiddenSpace
{
    public List<HiddenSpaceLocation> worldLocations = new List<HiddenSpaceLocation>();

    public HiddenSpaceLocation GetEmptyLocation()
    {
        var location = new HiddenSpaceLocation();
        return location;
    }

    public class HiddenSpaceLocation : IProductLocation
    {
        public Product Product { get; private set; }

        public bool Attach(Product product)
        {
            if (product != null)
                return false;
            Product = product;
            World.Instance.hiddenSpace.worldLocations.Add(this);
            return true;
        }

        public void Detach()
        {
            if (Product == null)
                return;
            Product = null;
            World.Instance.hiddenSpace.worldLocations.Remove(this);
        }
    }
}