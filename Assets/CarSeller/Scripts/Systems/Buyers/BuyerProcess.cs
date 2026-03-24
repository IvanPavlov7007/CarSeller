using System.Collections;
using UnityEngine;

public class BuyerProcess : IProcess
{
    public readonly CityEntity BuyerEntity;

    public readonly Buyer Buyer;
    public readonly Car Car;
    public BuyerProcess(Buyer buyer, Car car)
    {
        this.Buyer = buyer;
        this.Car = car;
        BuyerEntity = CityLocatorHelper.GetCityEntity(buyer);
    }

    public IEnumerator Run()
    {
        if (GameRules.CarCanBeSoldToBuyer.Check(Car, Buyer))
        {
            var result = G.TransactionProcessor.Process(new SellTransaction(Car, Buyer, Car.CalculatePrice()),
                new TransactionFeedbackLocation(TransactionLocationType.WorldSpace, BuyerEntity.Position.WorldPosition));
            if(result.Type != TransactionResultType.Success)
            {
                Debug.LogError($"Failed to sell car to buyer {Buyer}. Reason: {result.Data}");
            }
            G.VehicleController.ExitWorldVehicle();
        }
        yield break;
    }
}