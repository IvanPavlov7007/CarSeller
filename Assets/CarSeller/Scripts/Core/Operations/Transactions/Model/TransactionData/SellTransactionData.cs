public class SellTransactionData : ITransactionData
{
    public SellTransactionData(Car car, float price)
    {
        Car = car;
        Price = price;
    }

    public Car Car { get; private set; }
    public float Price { get; private set; }
}