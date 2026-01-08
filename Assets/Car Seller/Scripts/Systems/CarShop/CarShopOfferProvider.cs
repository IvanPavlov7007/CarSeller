using Sirenix.Utilities;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Car Shop offers depend on the given car as well,
/// so offers created on demand when requested.
/// </summary>
public class CarShopOfferProvider
{
    List<Car> carOptions;

    public CarShopOfferProvider(List<Car> carOptions)
    {
        this.carOptions = carOptions;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="givenCar"></param>
    /// <param name="carOptionsToExchangeTo"></param>
    /// <param name="commissionMoney"> money player pays additionally for transaction </param>
    /// <returns></returns>
    public CarShopOffer[] GetOffers(Car givenCar, float commissionMoney)
    {
        CarShopOffer[] offers = new CarShopOffer[carOptions.Count];
        for(int i = 0; i < carOptions.Count; i++)
        {
            Car receiveCar = carOptions[i];
            float moneyDelta = G.Economy.ProductPriceCalculator.Calculate(givenCar) - G.Economy.ProductPriceCalculator.Calculate(receiveCar) - commissionMoney;
            offers[i] = new CarShopOffer(receiveCar, givenCar, moneyDelta);
     
        }

        offers.Sort((a, b) => b.MoneyDelta.CompareTo(a.MoneyDelta));

        return offers;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="givenCar"> car the player gives into pool</param>
    /// <param name="receivedCar">car the player gets from the pool</param>
    public void SwapCar(Car givenCar, Car receivedCar)
    {
        Debug.Assert(carOptions.Contains(receivedCar));
        Debug.Assert(!carOptions.Contains(givenCar));

        carOptions.Remove(receivedCar);
        carOptions.Add(givenCar);
    }
}
