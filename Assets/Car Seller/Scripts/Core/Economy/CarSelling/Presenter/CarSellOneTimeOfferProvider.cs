using System.Collections.Generic;
using UnityEngine;

public class CarSellOneTimeOfferProvider
{
    public CarSellOffer GetOfferByCar(Car car)
    {
        Debug.Assert(car != null, "GetOfferByCar called with null car.");
        Debug.Assert(G.Player.Owns(car));

        return new CarSellOffer(car);
    }
}