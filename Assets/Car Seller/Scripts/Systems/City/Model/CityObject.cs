using UnityEngine;

/// <summary>
/// Base class for objects that only exist within the city environment.
/// Used by Profiles through ContentProvider
/// </summary>
public class CityObject : ILocatable
{
    public string Name { get; private set; }
    public string InfoText { get; set; }

    public City.CityLocation Location { get; private set; }
    public CityObject(string name, string infoText, ILocation location)
    {
        Name = name;
        InfoText = infoText;

        Debug.Assert(location != null);
        Debug.Assert(location is  City.CityLocation);
        Location = location as City.CityLocation;

        if (location.Attach(this))
            GameEvents.Instance.OnLocatableCreated?.Invoke(new LocatableCreatedEventData(this, location));
        else
            Debug.LogError($"Failed to attach city object {name} to location {location}");
    }

    public void Destroy()
    {
        Location.Detach();
        GameEvents.Instance.OnLocatableDestroyed?.Invoke(new LocatableDestroyedEventData(this));
    }
}