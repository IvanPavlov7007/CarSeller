public class ProductCreatedEventData
{
    public Product Product { get; private set; }
    public ProductCreatedEventData(Product product)
    {
        Product = product;
    }
}

public class ProductDestroyedEventData
{
    public Product Product { get; private set; }
    public ProductDestroyedEventData(Product product)
    {
        Product = product;
    }
}   

public class ProductLocationChangedEventData
{
    public ProductLocationChangedEventData(Product product, IProductLocation newLocation, IProductLocation oldLocation)
    {
        Product = product;
        NewLocation = newLocation;
        OldLocation = oldLocation;
    }

    public Product Product { get; private set; }
    public IProductLocation NewLocation { get; private set; }
    public IProductLocation OldLocation { get; set; }
}