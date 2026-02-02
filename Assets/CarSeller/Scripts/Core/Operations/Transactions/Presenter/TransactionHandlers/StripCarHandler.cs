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
        if(stripCarData.TargetWarehouse == null)
            return TransactionResult.InvalidTransaction("Invalid data: TargetWarehouse is null.");
        if(stripCarData.Car == null)
            return TransactionResult.InvalidTransaction("Invalid data: CarToStrip is null.");
        if(stripCarData.StrippingProcess == null)
            return TransactionResult.InvalidTransaction("Invalid data: StrippingProcess is null.");


        var process = stripCarData.StrippingProcess;
        process.Strip();

        if (!addStrippedParts(process.StrippedParts, stripCarData.TargetWarehouse,out StripResultData stripResultData))
        {
            return new TransactionResult(TransactionResultType.Failure, data: new WarehousePlacingFailureData(stripCarData.TargetWarehouse));
        }

        // Build result first
        var result = TransactionResult.Success(stripResultData);
        return result;
    }

    private bool addStrippedParts(IReadOnlyList<Product> strippedParts, Warehouse warehouse, out StripResultData stripResultData)
    {
        stripResultData = new StripResultData();

        foreach (var product in strippedParts)
        {
            //first check if there is space in the warehouse
            //for now only for cars
            if(product is Car)
            {
                if(warehouse.AvailableCarParkingSpots <= 0)
                {
                    stripResultData.carSkipped = true;
                    continue;
                }
            }

            if (!G.ProductLifetimeService.MoveProduct(product, warehouse.GetEmptyLocation()))
            {
                return false;
            }
        }
        return true;
    }
}

public class StripResultData : ITransactionResultData
{
    public bool carSkipped = false;

}