public class PurchaseTransactionData : ITransactionData
{
    public PurchaseTransactionData(float price, IPossession[] items)
    {
        Price = price;
        Items = items;
    }

    public float Price { get; private set; }
    public IPossession[] Items { get; private set; }
}