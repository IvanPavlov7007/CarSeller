using System;
using UnityEngine;

public abstract class GameState
{
    public Car FocusedCar { get; }
    public PlayerFigure PlayerFigure { get; }

    protected GameState(Car focusedCar)
    {
        FocusedCar = focusedCar;
    }
}

public class NeutralGameState : GameState
{
    public NeutralGameState(Car focusedCar) : base(focusedCar)
    {
    }
}

public class SellingGameState : GameState
{
    public readonly Car SellingCar;
    public readonly Buyer Buyer;
    public readonly Warehouse SellingWarehouse;
    public SellingGameState(Car sellingCar , Buyer buyer, Warehouse sellingWarehouse)
        : base(sellingCar)
    {
        SellingCar = sellingCar;
        Buyer = buyer;
        SellingWarehouse = sellingWarehouse;
    }
}

public class StealingGameState : GameState
{
    public readonly Car StealingCar;

    public StealingGameState(Car stealingCar)
        :base(stealingCar)
    {
        StealingCar = stealingCar;
    }
}

public class FreeRoamGameState : GameState
{
    public FreeRoamGameState(Car focusedCar) : base(focusedCar)
    {
    }
}

public class MissionGameState : GameState
{
    public readonly MissionRuntime CurrentMission;
    public MissionGameState(Car focusedCar, MissionRuntime currentMission)
        : base(focusedCar)
    {
        CurrentMission = currentMission;
    }
}