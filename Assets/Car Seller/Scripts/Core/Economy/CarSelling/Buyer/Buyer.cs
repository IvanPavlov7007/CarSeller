using UnityEngine;
//TODO use global Location Service to manage locatables

public class Buyer : ILocatable
{
    public string Name { get; private set; }
    public Buyer(string name, ILocation location)
    {
        Name = name;

        if(location.Attach(this))
            GameEvents.Instance.OnLocatableCreated?.Invoke(new LocatableCreatedEventData(this, location));
        else
            Debug.LogError($"Failed to attach buyer {name} to location {location}");
    }

    public void Destroy()
    {
        GameEvents.Instance.OnLocatableDestroyed?.Invoke(new LocatableDestroyedEventData(this));
    }
}