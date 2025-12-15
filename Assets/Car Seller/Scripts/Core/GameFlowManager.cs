using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameFlowManager : RoutinedObject
{

    G G=> G.Instance;
    GameFlowController GameFlowController => G.GameFlowController;

    public GameFlowManager() : base()
    {
        var go = new GameObject("GameFlowManagerRoutines");
        Object.DontDestroyOnLoad(go);
        G.GameState = new NormalGameState();
    }

    public void SellCar(Car car)
    {
        if(!TryStartRoutine(SellingSequence(car)))
        {
            Debug.LogWarning("A selling sequence is already in progress.");
        }
    }

    public void StealCar(Car car)
    {
        if(!TryStartRoutine(StealingSequence(car)))
        {
            Debug.LogWarning("A stealing sequence is already in progress.");
        }
    }

    private IEnumerator SellingSequence(Car car)
    {
        // Enter selling state
        GameFlowController.SetGameState(new SellingGameState(car));

        var initialWarehouse = G.LocationService.GetProductLocation(car) as Warehouse;
        // Move to the city/map
        G.CarMechanicService.RideCarFromWarehouse(car, initialWarehouse);

        // Wait for player outcome via events
        yield return AwaitPlayerOutcome(
            onCanceled: () =>
            {
                // Player canceled selling — return to normal state
            },
            onCaught: () =>
            {
                // Player got caught — handle penalty and return to normal (or specific) state
                HandleCaughtOutcome(car);
            },
            onSucceed: (warehouse) =>
            {
                // Player succeeded — move car to the selected warehouse, then normal state
                if (warehouse != null)
                {
                    new CityActionService().PutCarInsideWarehouse(car, warehouse);
                }
            }
        );
        GameFlowController.SetGameState(new NormalGameState());
    }

    private IEnumerator StealingSequence(Car car)
    {
        // Enter stealing state and move to the city/map
        GameFlowController.SetGameState(new StealingGameState(car));
        GameFlowController.GetToTheCity();

        // Wait for player outcome via events
        yield return AwaitPlayerOutcome(
            onCanceled: () =>
            {
                // Aborted theft — revert to normal state
            },
            onCaught: () =>
            {
                // Theft failed — apply consequences
                HandleCaughtOutcome(car);
            },
            onSucceed: (warehouse) =>
            {
                // Theft succeeded — stash car in a warehouse if provided
                if (warehouse != null)
                {
                    G.CityActionService.PutCarInsideWarehouse(car, warehouse);
                }
                
            }
        );
        GameFlowController.SetGameState(new NormalGameState());
    }

    private IEnumerator AwaitPlayerOutcome(System.Action onCanceled, System.Action onCaught, System.Action<Warehouse> onSucceed)
    {
        bool completed = false;

        System.Action<PlayerActionEventData> cancelHandler = (data) =>
        {
            if (completed) return;
            completed = true;
            onCanceled?.Invoke();
        };

        System.Action<PlayerActionEventData> caughtHandler = (data) =>
        {
            if (completed) return;
            completed = true;
            onCaught?.Invoke();
        };

        System.Action<PlayerActionEventData> succeedHandler = (data) =>
        {
            if (completed) return;
            completed = true;
            onSucceed?.Invoke(data.Warehouse);
        };

        GameEvents.Instance.OnPlayerCancel += cancelHandler;
        GameEvents.Instance.OnPlayerCaught += caughtHandler;
        GameEvents.Instance.OnPlayerSucceed += succeedHandler;

        // Wait until one of the handlers sets completed
        while (!completed)
        {
            yield return null;
        }

        // Unsubscribe
        GameEvents.Instance.OnPlayerCancel -= cancelHandler;
        GameEvents.Instance.OnPlayerCaught -= caughtHandler;
        GameEvents.Instance.OnPlayerSucceed -= succeedHandler;
    }

    private void HandleCaughtOutcome(Car car)
    {
        // TODO: implement penalties, confiscation, or state changes as appropriate
        // This is a placeholder hook to centralize "caught" logic.
    }
}