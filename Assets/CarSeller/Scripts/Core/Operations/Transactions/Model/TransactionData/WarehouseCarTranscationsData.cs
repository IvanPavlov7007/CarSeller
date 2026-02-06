using System.Collections.Generic;
using System.Linq;
using static WarehouseActionsHelper;

public class StripCarTransactionData : ITransactionData
{
    public readonly Car Car;
    public readonly ITargetProductsHolder TargetProductsHolder;
    public readonly StrippingProcess StrippingProcess;

    public StripCarTransactionData(Car car, ITargetProductsHolder targetProductsHolder, StrippingProcess strippingProcess)
    {
        Car = car;
        TargetProductsHolder = targetProductsHolder;
        StrippingProcess = strippingProcess;
    }
}

public interface ITargetProductsHolder
{
    ProductsPutInsideResult PutProducts(IReadOnlyList<Product> parts);
}

public class HiddenSpaceHolderAdapter : ITargetProductsHolder
{
    public ProductsPutInsideResult PutProducts(IReadOnlyList<Product> parts)
    {
        var putInsideProducts = new List<Product>();
        var skippedProducts = new List<Product>();
        skippedProducts.AddRange(parts);
        foreach (var part in parts)
        {
            var loc = World.Instance.HiddenSpace.GetEmptyLocation();
            if (G.ProductLifetimeService.MoveProduct(part, loc))
            {
                putInsideProducts.Add(part);
                skippedProducts.Remove(part);
            }
        }
        return new ProductsPutInsideResult(putInsideProducts, skippedProducts);
    }
}

public class WarehouseHolderAdapter : ITargetProductsHolder
{
    private readonly Warehouse warehouse;
    public WarehouseHolderAdapter(Warehouse warehouse)
    {
        this.warehouse = warehouse;
    }
    public ProductsPutInsideResult PutProducts(IReadOnlyList<Product> parts)
    {
        return WarehouseActionsHelper.TryPutProductsInsideWarehouse(warehouse, parts.ToArray());
    }
}

public class PullCarFromWarehouseTransactionData : ITransactionData
{
    public readonly Car Car;
    public readonly Warehouse SourceWarehouse;
    public PullCarFromWarehouseTransactionData(Car car, Warehouse sourceWarehouse)
    {
        Car = car;
        SourceWarehouse = sourceWarehouse;
    }
}

public class PutProductsInWarehouseTransactionData : ITransactionData
{
    public readonly Product[] Products;
    public readonly Warehouse TargetWarehouse;

    public PutProductsInWarehouseTransactionData(Warehouse targetWarehouse, params Product[] products)
    {
        Products = products;
        TargetWarehouse = targetWarehouse;
    }
}