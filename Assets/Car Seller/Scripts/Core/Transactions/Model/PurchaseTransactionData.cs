public class PurchaseTransactionData : ITransactionData
{
    public float Price { get; private set; }
    public IPossession[] Items { get; private set; }
}