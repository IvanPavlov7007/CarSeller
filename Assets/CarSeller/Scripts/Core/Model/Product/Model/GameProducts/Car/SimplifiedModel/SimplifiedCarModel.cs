using System;

[Serializable]
public enum CarColor
{
    Red,
    Green,
    Blue,
    Yellow
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
    Rare,
    Epic,
    Legendary
}

public interface ISimplifiedCarModel
{
    public CarColor Color { get; }
    public CarType Type { get; }
    public CarRarity Rarity { get; }
}