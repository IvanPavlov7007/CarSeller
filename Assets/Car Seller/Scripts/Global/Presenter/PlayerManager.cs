using System;
using UnityEngine;

public class PlayerManager
{
    Player Player => World.Instance.Economy.Player;

    public bool RemovePossession(IPossession possession)
    {
        if(!Player.Possessions.Contains(possession))
        {
            Debug.LogWarning("Player does not own this possession.");
            return false;
        }

        if(Player.Possessions.Remove(possession))
        { 
            GameEvents.Instance.OnPlayerPossessionLose?.Invoke(new PossesionChangeEventData(possession));
            return true;
        }
        else
        {
            Debug.LogWarning("Failed to remove possession from player.");
            return false;
        }
    }

    public bool AddPossession(IPossession possession)
    {
        if (Player.Possessions.Contains(possession))
        {
            Debug.LogWarning("Player already owns this possession.");
            return false;
        }
        if (Player.Possessions.Add(possession))
        {
            //TODO instead of having manager, make the ownership system, that checks things like this
            return addRecursiveStoredPossessions(possession);
        }
        else
        {
            Debug.LogWarning("Failed to add possession to player.");
            return false;
        }
    }

    bool addRecursiveStoredPossessions(IPossession possession)
    {
        GameEvents.Instance.OnPlayerPossessionAcquired?.Invoke(new PossesionChangeEventData(possession));
        bool cumulative = true;
        if(possession is ILocationsHolder container)
        {
            foreach (var location in container.GetLocations())
            {
                if(location.Occupant is IPossession childPossession)
                    cumulative &= AddPossession(childPossession);
            }
        }

        return cumulative;
    }

    public float SetPlayerMoney(float amount)
    {
        float oldMoney = Player.Money;
        Player.ChangeMoney(amount);
        GameEvents.Instance.OnPlayerMoneyChanged?.Invoke(new PlayerMoneyChangeEventData(Player, oldMoney, Player.Money));
        return Player.Money;
    }

    public float DeltaPlayerMoney(float delta)
    {
        return SetPlayerMoney(Player.Money + delta);
    }

    public float AddPlayerMoney(float amount)
    {
        return SetPlayerMoney(Player.Money + amount);
    }

    public float SubtractPlayerMoney(float amount)
    {
        return SetPlayerMoney(Player.Money - amount);
    }

    public void RegisterPlayerPossession(IPossession possession)
    {
        Debug.Assert(possession != null, "Possession cannot be null when registering to player.");
        Debug.Assert(!Player.Possessions.Contains(possession), $"Possession {possession.Name} is already registered to player.");
        Player.Possessions.Add(possession);
    }

    internal void AddPossessions(IPossession[] items)
    {
        foreach (var item in items)
        {
            AddPossession(item);
        }
    }
}

