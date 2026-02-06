using System;
using UnityEngine;

public abstract class GameState
{
    public Car FocusedCar { get; }
    public PlayerFigure PlayerFigure { get; }

    public bool IsControlledByPlayer(ILocatable locatable)
    {
        return locatable == PlayerFigure || locatable == FocusedCar;
    }

    protected GameState(Car focusedCar, PlayerFigure playerFigure = null)
    {
        FocusedCar = focusedCar;
        PlayerFigure = playerFigure;
    }
}

public class NeutralGameState : GameState
{
    public NeutralGameState(Car focusedCar, PlayerFigure playerFigure = null) : base(focusedCar, playerFigure)
    {
    }
}

public class SellingGameState : GameState
{
    public readonly Car SellingCar;
    public readonly Buyer Buyer;
    public readonly Warehouse SellingWarehouse;
    public SellingGameState(Car sellingCar , Buyer buyer, Warehouse sellingWarehouse, PlayerFigure playerFigure = null)
        : base(sellingCar, playerFigure)
    {
        SellingCar = sellingCar;
        Buyer = buyer;
        SellingWarehouse = sellingWarehouse;
    }
}

public class StealingGameState : GameState
{
    public readonly Car StealingCar;

    public StealingGameState(Car stealingCar, PlayerFigure playerFigure = null)
        :base(stealingCar, playerFigure)
    {
        StealingCar = stealingCar;
    }
}

public class FreeRoamGameState : GameState
{
    public FreeRoamGameState(Car focusedCar, PlayerFigure playerFigure = null) : base(focusedCar, playerFigure)
    {
    }
}

public class MissionGameState : GameState
{
    public readonly MissionRuntime CurrentMission;
    public MissionGameState(Car focusedCar, MissionRuntime currentMission, PlayerFigure playerFigure = null)
        : base(focusedCar, playerFigure)
    {
        CurrentMission = currentMission;
    }
}