public abstract class GameState
{

}

public class NormalGameState : GameState
{
    
}

public class SellingGameState : GameState
{
    public readonly Car SellingCar;
    public SellingGameState(Car sellingCar)
    {
        SellingCar = sellingCar;
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