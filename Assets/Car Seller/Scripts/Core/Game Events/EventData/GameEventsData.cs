public class ProductCreatedEventData
{
    public Product Product { get; private set; }
    public ILocation Location { get; private set; }
    public ProductCreatedEventData(Product product, ILocation location)
    {
        Product = product;
        Location = location;
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
    public ProductLocationChangedEventData(Product product, ILocation newLocation, ILocation oldLocation)
    {
        Product = product;
        NewLocation = newLocation;
        OldLocation = oldLocation;
    }

    public Product Product { get; private set; }
    public ILocation NewLocation { get; private set; }
    public ILocation OldLocation { get; set; }
}

public class GameStateChangeEventData
{
    private GameState oldState;
    private GameState newState;

    public GameStateChangeEventData(GameState oldState, GameState newState)
    {
        this.oldState = oldState;
        this.newState = newState;
    }
}

public class PossesionChangeEventData
{
    public IPossession Possession { get; private set; }
    public PossesionChangeEventData(IPossession possesion)
    {
        Possession = possesion;
    }
}