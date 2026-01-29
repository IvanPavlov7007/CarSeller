public class RewardTransactionData : ITransactionData
{
    public RewardTransactionData(float price, IPossession[] items)
    {
        Price = price;
        Items = items;
    }
    public float Price { get; private set; }
    public IPossession[] Items { get; private set; }
}