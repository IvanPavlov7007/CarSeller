public class ExchangeTransactionData : ITransactionData
{
    public ExchangeTransactionData(float deltaMoney, Car givenCar, Car receivedCar)
    {
        DeltaMoney = deltaMoney;
        GivenCar = givenCar;
        ReceivedCar = receivedCar;
    }

    public float DeltaMoney { get; private set; }
    public Car GivenCar { get; private set; }
    public Car ReceivedCar { get; private set; }
}