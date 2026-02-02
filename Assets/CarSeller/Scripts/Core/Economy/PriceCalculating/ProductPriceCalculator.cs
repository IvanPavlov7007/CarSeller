using UnityEngine;

//TODO add modifiers, discounts, dynamic pricing etc

public class ProductPriceCalculator
{
    public float Calculate(Car car)
    {
        float totalPrice = car.BasePrice;
        // Add prices of all parts in slots
        foreach (var location in car.GetLocations())
        {
            if (location.Occupant != null)
            {
                switch (location.Occupant)
                {
                    case CarFrame carFrame:
                        totalPrice += Calculate(carFrame);
                        break;
                    case Engine engine:
                        totalPrice += Calculate(engine);
                        break;
                    case Wheel wheel:
                        totalPrice += Calculate(wheel);
                        break;
                    case Spoiler spoiler:
                        totalPrice += Calculate(spoiler);
                        break;
                    default:
                        Debug.LogWarning("Unknown product type in slot.");
                        break;
                }
            }
        }
        return totalPrice;
    }

    public float Calculate(CarFrame carFrame)
    {
        return carFrame.BasePrice;
    }

    public float Calculate(Engine engine)
    {
        return engine.BasePrice;
    }

    public float Calculate(Spoiler spoiler)
    {
        return spoiler.BasePrice;
    }

    public float Calculate(Wheel wheel)
    {
        return wheel.BasePrice;
    }
}