using UnityEngine;

public class CarShopOffer : IOffer
{
    public Car ReceivedCar { get; private set; }
    public Car GivenCar { get; private set; }

    public float MoneyDelta;

    public bool Accepted { get; private set; } = false;

    public CarShopOffer(
        Car receivedCar,
        Car givenCar,
        float moneyDelta
    )
    {
        ReceivedCar = receivedCar;
        GivenCar = givenCar;
        MoneyDelta = moneyDelta;
    }
    //TODO in the future add context to check if the offer can be accepted
    //and modify price based on that context
    public Transaction Accept()
    {
        Debug.Assert(!Accepted);
        Debug.Assert(CanAccept(), "Cannot accept offer: car not available.");
        var exchangeData = new ExchangeTransactionData(MoneyDelta,ReceivedCar, GivenCar);
        Transaction transaction = new Transaction(TransactionType.Purchase, exchangeData);
        Accepted = true;
        return transaction;
    }
    public bool CanAccept()
    {
        return !Accepted && G.Player.Money + MoneyDelta >= 0f;
    }
}