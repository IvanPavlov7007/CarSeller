using UnityEngine;

public enum CityObjectVisualState
{
    Normal,
    Disabled,
}

public abstract class CitySceneProfile
{
    public abstract bool ShouldShow(object obj, GameState gameState);

    public virtual CityObjectVisualState GetObjectVisualState(object obj, GameState gameState)
    {
        return CityObjectVisualState.Normal;
    }

    public virtual void OnProfileActivated(GameState gameState) { }
    public virtual void OnProfileDeactivated() { }
}


public sealed class NormalCitySceneProfile : CitySceneProfile
{
    public override bool ShouldShow(object obj, GameState gameState)
    {
        return true;
    }
}

public sealed class StealingCitySceneProfile : CitySceneProfile
{
    public override bool ShouldShow(object obj, GameState gameState)
    {
        Debug.Assert(gameState is StealingGameState, "StealingCitySceneProfile: gameState is not StealingGameState");
        var stealingState = gameState as StealingGameState;


        //TODO add police
        switch (obj)
        {
            case Car car:
                return car == stealingState.StealingCar;
            case Warehouse warehouse:
                {
                    if (!G.Player.Owns(warehouse))
                        return false;
                    return true;
                }
            default:
                Debug.LogError($"StealingCitySceneProfile: Unsupported object type {obj.GetType()}");
                return false;
        }
    }

    public override CityObjectVisualState GetObjectVisualState(object obj, GameState gameState)
    {
        switch (obj)
        {
            case Warehouse warehouse:
                {
                    var stealingState = gameState as StealingGameState;
                    if (warehouse.AvailableCarParkingSpots > 0)
                        return CityObjectVisualState.Normal;
                    else
                        return CityObjectVisualState.Disabled;
                }
            default:
                {
                    return CityObjectVisualState.Normal;
                }
        }
    }
}

public sealed class SellingCitySceneProfile : CitySceneProfile
{
    public override bool ShouldShow(object obj, GameState gameState)
    {
        Debug.Assert(gameState is SellingGameState, "SellingCitySceneProfile: gameState is not SellingGameState");
        var sellingState = gameState as SellingGameState;
        switch (obj)
        {
            case Car car:
                return car == sellingState.SellingCar;
            case Warehouse warehouse:
                {
                    if (!G.Player.Owns(warehouse))
                        return false;
                    return true;
                }
            default:
                Debug.LogError($"SellingCitySceneProfile: Unsupported object type {obj.GetType()}");
                return false;
        }
    }
}