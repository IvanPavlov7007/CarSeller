using Pixelplacement;
using UnityEngine;

public class CarShopSceneManager : Singleton<CarShopSceneManager>
{
    CarShopOfferProvider OfferProvider => G.Economy.CarShopOfferProvider;

    private void OnEnable()
    {
        GameEvents.Instance.OnTransactionComplete += onTransactionComplete;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnTransactionComplete -= onTransactionComplete;
    }


    public void ExitCarShop()
    {
        var warehouse = WarehouseSceneManager.SceneWarehouseModel;
        var state = G.GameState as FreeRoamGameState;
        if (state == null)
        {
            Debug.LogError("ExitCarShop: GameState is not FreeRoamGameState");
            return;
        }

        var car = findCurrentCar();

        //TODO maybe move this into CarMechanicService or make common for all states and exits
        state.FocusedCar = car;
        state.NotifyExitedWarehouse();
        G.Instance.CarMechanicService.RideCarFromWarehouse(car, warehouse);
    }

    Car findCurrentCar()
    {
        var warehouse = WarehouseSceneManager.SceneWarehouseModel;
        var cars = warehouse.GetCars();
        if (cars.Count == 0)
        {
            Debug.LogError("ExitCarShop: No cars in warehouse to exit with.");
            return null;
        }
        return cars[0];
    }

    void updateOffers()
    {
        var offers = OfferProvider.GetOffers(findCurrentCar(), 500f);
        CarShopUIManager.Instance.UpdateOffers(offers);
    }


    void onTransactionComplete(TransactionEventData eventData)
    {
        Debug.Assert(eventData != null, "onTransactionComplete received null eventData.");
        Debug.Assert(eventData.Transaction != null, "onTransactionComplete received null Transaction.");
        if (eventData.Transaction.Type != TransactionType.Exchange)
        {
            return;
        }
        if(eventData.Transaction.Result.Type != TransactionResultType.Success)
        {
            Debug.LogError("onTransactionComplete: Exchange transaction failed with result " + eventData.Transaction.Result.Type);
            return;
        }
        var exchangeData = eventData.Transaction.Data as ExchangeTransactionData;

        OfferProvider.SwapCar(exchangeData.GivenCar, exchangeData.ReceivedCar);
        updateOffers();
    }
}