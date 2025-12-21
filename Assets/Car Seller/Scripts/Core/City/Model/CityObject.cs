using UnityEngine;

/// <summary>
/// Base class for objects that only exist within the city environment.
/// Used by Profiles through ContentProvider
/// </summary>
public class CityObject : ILocatable
{
    public string Name { get; private set; }
    public string InfoText { get; set; }
    public CityObject(string name, string infoText, ILocation location)
    {
        Name = name;
        InfoText = infoText;
        if (location.Attach(this))
            GameEvents.Instance.OnLocatableCreated?.Invoke(new LocatableCreatedEventData(this, location));
        else
            Debug.LogError($"Failed to attach buyer {name} to location {location}");
    }

    public void Destroy()
    {
        GameEvents.Instance.OnLocatableDestroyed?.Invoke(new LocatableDestroyedEventData(this));
    }
}