public class Transaction
{
    public Transaction(TransactionType type, ITransactionData data)
    {
        Type = type;
        Data = data;
    }

    public void FinalizeResult(TransactionResult result)
    {
        Result = result;
    }   

    public TransactionType Type { get; private set; }
    public ITransactionData Data { get; private set; }
    public TransactionResult Result { get; private set; }

}


public interface ITransactionData { }

public enum TransactionType
{
    Purchase,
    Steal,
    Sell,
    Reward,
    Lose,
    Confiscate
}

public class TransactionResult
{
    public TransactionResultType Type { get; private set; }
    public ITransactionResultData Data { get; private set; }
    public TransactionResult(TransactionResultType type, ITransactionResultData data = null)
    {
        Type = type;
        Data = data;
    }

    public static TransactionResult Success(ITransactionResultData data = null)
    {
        return new TransactionResult(TransactionResultType.Success, data);
    }

    public static TransactionResult InvalidTransaction(string text)
    {
        return new TransactionResult(TransactionResultType.InvalidTransaction, new MessageTransactionResultData(text));
    }
}

public enum TransactionResultType
{
    Success,
    Failure,
    InvalidTransaction,
} 

public interface ITransactionResultData { }

public class MessageTransactionResultData : ITransactionResultData
{
    public string Message { get; private set; }
    public MessageTransactionResultData(string message)
    {
        Message = message;
    }
}