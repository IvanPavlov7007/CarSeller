using System.Numerics;

public class RewardTransactionData : ITransactionData
{
    public RewardTransactionData(float price, IPossession[] items, TransactionLocation location)
    {
        Price = price;
        Items = items;
        Location = location;
    }
    public float Price { get; private set; }
    public IPossession[] Items { get; private set; }
    public TransactionLocation Location { get; private set; }
}