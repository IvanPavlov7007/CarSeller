public enum PlayerOutcome
{
    Canceled,
    Caught,
    Succeeded
}

public class PlayerActionEventData
{
    public PlayerOutcome Outcome { get; private set; }
    public Warehouse Warehouse { get; private set; } // only set when Outcome == Succeeded

    public PlayerActionEventData(PlayerOutcome outcome, Warehouse warehouse = null)
    {
        Outcome = outcome;
        Warehouse = warehouse;
    }
}