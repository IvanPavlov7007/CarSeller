using System;
using UnityEngine;

public abstract class GameState
{
    public Car FocusedCar;
}

public class NeutralGameState : GameState
{
    
}

public class SellingGameState : GameState
{
    public readonly Car SellingCar;
    public readonly Buyer Buyer;
    public readonly Warehouse SellingWarehouse;
    public SellingGameState(Car sellingCar , Buyer buyer, Warehouse sellingWarehouse)
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
    {
        StealingCar = stealingCar;
    }
}

public class FreeRoamGameState : GameState
{
    bool _skipNextWarehouseEntry;

    public FreeRoamGameState(Car focusedCar)
    {
        FocusedCar = focusedCar;
    }

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