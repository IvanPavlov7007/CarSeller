using System.Collections.Generic;

public class Player
{
    public float Money { get; private set; }
    public HashSet<IPossession> Possessions { get; private set; }
}

