using System;

[Serializable]
public enum CarColor
{
    Red,
    Green,
    Blue,
    Yellow, 
    Unique
}
[Serializable]
public enum CarType
{
    Sedan,
    Mini,
    Bike,
    Truck,
    Super
}
[Serializable]
public enum CarRarity
{
    Common,
    Uncommon,
    Epic
}

public interface ISimplifiedCarModel
{
    public CarColor Color { get; }
    public CarType Type { get; }
    public CarRarity Rarity { get; }
}