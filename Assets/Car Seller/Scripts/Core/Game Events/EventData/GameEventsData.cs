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

public class LocatableStateChangedEventData
{
    public ILocatable Locatable { get; private set; }
    public LocatableStateChangedEventData(ILocatable locatable)
    {
        Locatable = locatable;
    }
}

public class LocatableCreatedEventData
{
    public ILocatable Locatable { get; private set; }
    public ILocation Location { get; private set; }
    public LocatableCreatedEventData(ILocatable locatable, ILocation location)
    {
        Locatable = locatable;
        Location = location;
    }
}

public class LocatableDestroyedEventData
{
    public ILocatable Locatable { get; private set; }
    public LocatableDestroyedEventData(ILocatable locatable)
    {
        Locatable = locatable;
    }
}

public class LocatableLocationChangedEventData
{
    public LocatableLocationChangedEventData(ILocatable locatable, ILocation newLocation, ILocation oldLocation)
    {
        Locatable = locatable;
        NewLocation = newLocation;
        OldLocation = oldLocation;
    }

    public ILocatable Locatable { get; private set; }
    public ILocation NewLocation { get; private set; }
    public ILocation OldLocation { get; set; }
}

public class GameStateChangeEventData
{
    public GameState oldState { get; private set; }
    public GameState newState { get; private set; }

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

public class PlayerMoneyChangeEventData
{
    public PlayerMoneyChangeEventData(Player player, float oldMoney, float newMoney)
    {
        Player = player;
        OldMoney = oldMoney;
        NewMoney = newMoney;
    }

    public Player Player { get; private set; }
    public float OldMoney { get; private set; }
    public float NewMoney { get; private set; }
    
}