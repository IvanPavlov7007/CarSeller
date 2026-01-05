using UnityEngine;

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
    public TransactionLocation Location { get; set; }
    public TransactionResult(TransactionResultType type, TransactionLocation location = null, ITransactionResultData data = null)
    {
        Type = type;
        Data = data;
        if (location == null)
            location = TransactionLocation.OmniDirectional;
        Location = location;
    }

    public static TransactionResult Success(TransactionLocation location = null, ITransactionResultData data = null)
    {
        return new TransactionResult(TransactionResultType.Success, location, data);
    }

    public static TransactionResult InvalidTransaction(string text)
    {
        return new TransactionResult(TransactionResultType.InvalidTransaction, data: new MessageTransactionResultData(text));
    }

}

public class  TransactionLocation
{
    public static readonly TransactionLocation OmniDirectional = new TransactionLocation(TransactionLocationType.OmniDirectional, Vector3.zero);

    public TransactionLocation(TransactionLocationType type, Vector3 position)
    {
        Type = type;
        Position = position;
    }
    public TransactionLocationType Type { get; private set; }
    public Vector3 Position { get; private set; }
}

public enum TransactionLocationType
{
    OmniDirectional,
    ScreenSpace,
    WorldSpace
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