using UnityEngine;

public class PlayerManager
{
    Player Player => World.Instance.Player;

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
            GameEvents.Instance.OnPlayerPossessionAcquired?.Invoke(new PossesionChangeEventData(possession));
            return true;
        }
        else
        {
            Debug.LogWarning("Failed to add possession to player.");
            return false;
        }
    }

    public void RegisterPlayerPossession(IPossession possession)
    {
        Debug.Assert(possession != null, "Possession cannot be null when registering to player.");
        Debug.Assert(!Player.Possessions.Contains(possession), $"Possession {possession.Name} is already registered to player.");
        Player.Possessions.Add(possession);
    }
}

