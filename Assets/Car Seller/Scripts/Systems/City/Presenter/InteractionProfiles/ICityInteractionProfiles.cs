using System;
using UnityEngine;

public interface ICityContextMenuProfile
{
    /// <summary>
    /// Build context menu for a clicked city model in a given game state.
    /// </summary>
    UIElement GenerateContent(object model, GameState gameState);
}

public interface ICityTriggerProfile
{
    /// <summary>
    /// Decide if a trigger interaction can proceed and, if so, what it should do.
    /// </summary>
    TriggerAction GenerateTriggerAction(
        object trigger,       // the trigger object (e.g. Warehouse, Buyer, etc.)
        object triggerCause,  // the cause object (e.g. Car, player, etc.)
        GameState gameState);
}

/// <summary>
/// Registry for context menu profiles per GameState type.
/// </summary>
public sealed class CityContextMenuProfileRegistry
{
    private readonly System.Collections.Generic.Dictionary<Type, ICityContextMenuProfile> _profiles;

    public CityContextMenuProfileRegistry()
    {
        _profiles = new()
        {
            { typeof(NeutralGameState),  new NormalCityContextMenuProfile()  },
            { typeof(StealingGameState), new StealingCityContextMenuProfile() },
            { typeof(SellingGameState),  new SellingCityContextMenuProfile()  },
            { typeof(FreeRoamGameState), new FreeRoamCityContextMenuProfile() },
        };
    }

    public ICityContextMenuProfile Get(GameState state)
    {
        if (state == null)
        {
            Debug.LogError("CityContextMenuProfileRegistry.Get: state is null");
            return null;
        }

        var type = state.GetType();
        if (_profiles.TryGetValue(type, out var profile))
        {
            return profile;
        }

        Debug.LogError($"CityContextMenuProfileRegistry.Get: No profile registered for state type {type.Name}");
        return null;
    }
}

/// <summary>
/// Registry for trigger profiles per GameState type.
/// </summary>
public sealed class CityTriggerProfileRegistry
{
    private readonly System.Collections.Generic.Dictionary<Type, ICityTriggerProfile> _profiles;

    public CityTriggerProfileRegistry()
    {
        _profiles = new()
        {
            { typeof(NeutralGameState),  new NormalCityTriggerProfile()  },
            { typeof(StealingGameState), new StealingCityTriggerProfile() },
            { typeof(SellingGameState),  new SellingCityTriggerProfile()  },
            { typeof(FreeRoamGameState), new FreeRoamCityTriggerProfile() },
        };
    }

    public ICityTriggerProfile Get(GameState state)
    {
        if (state == null)
        {
            Debug.LogError("CityTriggerProfileRegistry.Get: state is null");
            return null;
        }

        var type = state.GetType();
        if (_profiles.TryGetValue(type, out var profile))
        {
            return profile;
        }

        Debug.LogError($"CityTriggerProfileRegistry.Get: No profile registered for state type {type.Name}");
        return null;
    }
}