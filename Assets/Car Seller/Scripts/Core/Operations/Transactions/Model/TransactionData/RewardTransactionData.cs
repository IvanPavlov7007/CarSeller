public class RewardTransactionData : ITransactionData
{
    public RewardTransactionData(float price, IOwnable[] items)
    {
        Price = price;
        Items = items;
    }
    public float Price { get; private set; }
    public IOwnable[] Items { get; private set; }
}