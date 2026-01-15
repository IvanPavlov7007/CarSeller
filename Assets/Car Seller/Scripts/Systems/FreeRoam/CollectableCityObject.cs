using System;

public class CollectableCityObject : CityObject
{
    //TODO make this betteer
    public CollectableCityObject(Collectable collectable, ILocation location, City.CityMarker cityMarker)
        : base(collectable.Name, collectable.InfoText, location, cityMarker, collectable)
    {
    }
    
}
[Serializable]
public class Collectable : CityObjectData
{
    public string InfoText { get; set; } = "Collectable Info";
    public string Name { get; set; } = "Collectable";
    public IPossession[] possessions { get; set; }
    public float MoneyAmount { get; set; }
    public event Action OnCollectedAdditionalCallback;
    // please don't put here meta data related to city object system to avoid circular dependencies
    public object customData { get; set; }
}