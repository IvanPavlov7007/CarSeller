public class SellTransaction : Transaction
{
    public SellTransaction(Car car, float price)
    {
        Car = car;
        Price = price;
    }

    public readonly Car Car;
    public readonly float Price;
}