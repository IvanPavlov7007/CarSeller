using System.Collections.Generic;
using UnityEngine;

public class StripCarHandler : TransactionHandler
{
    public override bool CanHandle(Transaction transaction) => transaction.Type == TransactionType.StripCar;
    public override TransactionResult Handle(Transaction transaction)
    {
        // Programming error if the processor routed a non-strip car transaction here.
        Debug.Assert(transaction != null && transaction.Type == TransactionType.StripCar,
            "StripCarHandler received a non-strip car transaction.");
        if (transaction == null || transaction.Type != TransactionType.StripCar)
            return TransactionResult.InvalidTransaction("Invalid transaction: expected StripCar.");
        var stripCarData = transaction.Data as StripCarTransactionData;
        if (stripCarData == null)
            return TransactionResult.InvalidTransaction("Invalid data: expected StripCarTransactionData.");
        if(stripCarData.TargetProductsHolder == null)
            return TransactionResult.InvalidTransaction("Invalid data: TargetWarehouse is null.");
        if(stripCarData.Car == null)
            return TransactionResult.InvalidTransaction("Invalid data: CarToStrip is null.");
        if(stripCarData.StrippingProcess == null)
            return TransactionResult.InvalidTransaction("Invalid data: StrippingProcess is null.");


        var process = stripCarData.StrippingProcess;
        process.Strip();

        var resultData = new StripResultData(addStrippedParts(process.StrippedParts, stripCarData.TargetProductsHolder));

        // Build result first
        var result = TransactionResult.Success(resultData);
        return result;
    }

    private ProductsPutInsideResult addStrippedParts(IReadOnlyList<Product> strippedParts, ITargetProductsHolder targetProductsHolder)
    {
        return targetProductsHolder.PutProducts(strippedParts);
    }
}

public class StripResultData : ITransactionResultData
{
    public readonly ProductsPutInsideResult PutInsideResult;
    public StripResultData(ProductsPutInsideResult putInsideResult)
    {
        PutInsideResult = putInsideResult;
    }
}