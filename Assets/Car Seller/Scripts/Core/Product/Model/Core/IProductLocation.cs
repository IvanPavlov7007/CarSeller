using System.Linq;

public interface IProductLocation
{
    // Usual structure:
    // 1) location
    // 2) position in the local spacial system
    // 3) product
    Product Product { get; }

    IProductsHolder Holder { get; }

    bool Attach(Product product);
    void Detach();
}

public interface IProductsHolder
{
    IProductLocation[] GetProductLocations();
}

public static class ProductsHolderExtensions
{
    public static IProductLocation[] GetNonEmptyProductLocations(this IProductsHolder holder)
        => holder.GetProductLocations().Where(l => l.Product != null).ToArray();
}