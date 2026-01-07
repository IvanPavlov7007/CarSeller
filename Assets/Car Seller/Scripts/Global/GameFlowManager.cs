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
    }

    public void SellCar(CarSellOffer offer)
    {

        if(!TryStartRoutine(SellingSequence(offer)))
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

    private IEnumerator SellingSequence(CarSellOffer offer)
    {
        Debug.Assert(G.GameFlowController.currentSceneType == GameFlowController.GameSceneType.Warehouse);

        var car = offer.Car;
        var warehouse = CityLocatorHelper.GetWarehouse(car);
        var buyer = BuyerManager.CreateBuyer(car, offer);

        // Enter selling state
        GameFlowController.SetGameState(new SellingGameState(car, buyer, warehouse));

        var initialWarehouse = CityLocatorHelper.GetWarehouse(car);
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
            // Player succeeded — apply selling transaction and move to the warehouse
            onSucceed: (_) =>
            {
                Transaction sellingTransaction = offer.Accept();

                var feedbackLocation = new TransactionFeedbackLocation(TransactionLocationType.WorldSpace,
                CityLocatorHelper.GetCityLocation(car).CityPosition.WorldPosition);
                var result = G.TransactionProcessor.Process(sellingTransaction, feedbackLocation);
                if(result.Type != TransactionResultType.Success)
                {
                    Debug.LogError("Failed to process stealing transaction during selling sequence.");
                }
            }
        );

        buyer.Destroy();
        GameFlowController.SetGameState(new NeutralGameState());
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
                Debug.Assert(warehouse != null, "StealingSequence: warehouse is null on succeed.");
                Debug.Assert(car != null, "StealingSequence: car is null on succeed.");

                var location = new TransactionFeedbackLocation(TransactionLocationType.WorldSpace,
                    CityLocatorHelper.GetCityLocation(car).CityPosition.WorldPosition);
                G.TransactionProcessor.Process(
                    new Transaction(TransactionType.Steal, new StealTransactionData(car, warehouse)),
                    location
                );
            }
        );
        GameFlowController.SetGameState(new NeutralGameState());
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