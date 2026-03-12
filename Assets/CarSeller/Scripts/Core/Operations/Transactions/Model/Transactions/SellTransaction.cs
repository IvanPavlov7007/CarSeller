public class SellTransaction : Transaction
{
    public SellTransaction(Car car, Buyer buyer, float price)
    {
        Car = car;
        Price = price;
        Buyer = buyer;

    }

    public readonly Buyer Buyer;
    public readonly Car Car;
    public readonly float Price;
}