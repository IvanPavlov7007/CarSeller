public class PurchaseTransactionData : ITransactionData
{
    public PurchaseTransactionData(float price, IMutableOwnable[] items)
    {
        Price = price;
        Items = items;
    }

    public float Price { get; private set; }
    public IMutableOwnable[] Items { get; private set; }
}