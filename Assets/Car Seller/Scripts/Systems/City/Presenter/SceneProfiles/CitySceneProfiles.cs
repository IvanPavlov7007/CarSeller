using UnityEngine;

public struct CityObjectState
{
    public ViewObjectVisualState visualState;
    public bool draggable;
    public bool interactable;

    public static CityObjectState Default =>
        new CityObjectState(ViewObjectVisualState.Normal, true, true);

    public CityObjectState(ViewObjectVisualState visualState = ViewObjectVisualState.Normal, bool draggable = true, bool interactable = true)
    {
        this.visualState = visualState;
        this.draggable = draggable;
        this.interactable = interactable;
    }
}

public abstract class CitySceneProfile
{
    public abstract bool ShouldShow(object obj, GameState gameState);

    public virtual CityObjectState GetObjectViewState(object obj, GameState gameState)
    {
        return CityObjectState.Default;
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

    public override CityObjectState GetObjectViewState(object obj, GameState gameState)
    {
        switch (obj)
        {
            // disable dragging for cars in normal profile
            case Car car:
                return new CityObjectState(ViewObjectVisualState.Normal, draggable: false);
        }

        return base.GetObjectViewState(obj, gameState);
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
                    return true;
                }
            default:
                Debug.LogError($"StealingCitySceneProfile: Unsupported object type {obj.GetType()}");
                return false;
        }
    }

    public override CityObjectState GetObjectViewState(object obj, GameState gameState)
    {
        switch (obj)
        {
            case Warehouse warehouse:
                {
                    var stealingState = gameState as StealingGameState;
                    if (warehouse.AvailableCarParkingSpots > 0)
                        return CityObjectState.Default;
                    else
                        return new CityObjectState(ViewObjectVisualState.Disabled);
                }
            default:
                {
                    return base.GetObjectViewState(obj, gameState);
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
                    return warehouse == sellingState.SellingWarehouse;
                }
            case Buyer buyer:
                return buyer == sellingState.Buyer;
            default:
                Debug.LogError($"SellingCitySceneProfile: Unsupported object type {obj.GetType()}");
                return false;
        }
    }
}