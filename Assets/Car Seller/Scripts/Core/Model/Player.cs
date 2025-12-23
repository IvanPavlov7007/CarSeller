using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public float Money { get; private set; }

    internal void ChangeMoney(float newAmount)
    {
        Debug.Assert(newAmount >= 0, "Player money cannot be negative.");
        Money = newAmount;
    }
    public bool Owns(IPossession possession)
    {
        return Possessions.Contains(possession);
    }

    internal HashSet<IPossession> Possessions { get; private set; } = new HashSet<IPossession>();
}

