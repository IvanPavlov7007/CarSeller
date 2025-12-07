using NUnit.Framework;
using System.Collections.Generic;

public class World
{
    public HiddenSpace HiddenSpace = new HiddenSpace();
    public Dictionary<Product, ILocation> productLocations = new Dictionary<Product, ILocation>();
    public City City;

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
public class HiddenSpace : ILocationsHolder
{
    public List<HiddenSpaceLocation> hiddenLocations = new List<HiddenSpaceLocation>();

    public HiddenSpaceLocation GetEmptyLocation()
    {
        var location = new HiddenSpaceLocation();
        return location;
    }

    public ILocation[] GetLocations()
    {
        return hiddenLocations.ToArray();
    }

    public class HiddenSpaceLocation : ILocation
    {
        public ILocatable Occupant { get; private set; }

        public ILocationsHolder Holder => World.Instance.HiddenSpace;

        public bool Attach(ILocatable locatable)
        {
            if (locatable != null)
                return false;
            Occupant = locatable;
            World.Instance.HiddenSpace.hiddenLocations.Add(this);
            return true;
        }

        public void Detach()
        {
            if (Occupant == null)
                return;
            Occupant = null;
            World.Instance.HiddenSpace.hiddenLocations.Remove(this);
        }
    }
}