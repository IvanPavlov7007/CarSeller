using System;
using UnityEngine;

public abstract class GameState
{
    public Car FocusedCar;

    public GameState(Car focusedCar)
    {
        FocusedCar = focusedCar;
    }

    bool _skipNextWarehouseEntry;

    public bool CanEnterWarehouse(Warehouse warehouse, Car car)
    {
        if (car != FocusedCar)
            return false;

        if (_skipNextWarehouseEntry)
        {
            _skipNextWarehouseEntry = false;
            return false;
        }

        return true;
    }

    public void NotifyExitedWarehouse()
    {
        _skipNextWarehouseEntry = true;
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