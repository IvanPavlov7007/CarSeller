public abstract class GameState
{

}

public class NeutralGameState : GameState
{
    
}

public class SellingGameState : GameState
{
    public readonly Car SellingCar;
    public readonly Buyer Buyer;
    public SellingGameState(Car sellingCar , Buyer buyer)
    {
        SellingCar = sellingCar;
        Buyer = buyer;
    }
}

public class StealingGameState : GameState
{
    public readonly Car StealingCar;
    public StealingGameState(Car stealingCar)
    {
        StealingCar = stealingCar;
    }
}