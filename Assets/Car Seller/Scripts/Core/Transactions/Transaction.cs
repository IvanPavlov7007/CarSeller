public class Transaction
{
    public TransactionType Type { get; private set; }
    public ITransactionData Data { get; private set; }
    public TransactionResult Result { get; private set; }

}


public interface ITransactionData { }

public enum TransactionType
{
    Purchase,
    Sell,
    Reward,
    Lose,
    Confiscate
}

public enum TransactionResult
{
    Success,
    InvalidTransaction
} 