using System.Linq;
using UnityEngine;

public class CarShopGameMain : GameMain
{
    Car car;
    CarShopGameModeData modeData;
    bool firstLoaded = false;

    public override void InitializeWorld(GameConfig gameConfig)
    {
        base.InitializeWorld(gameConfig);

        Debug.Assert(gameConfig.GameModeData is CarShopGameModeData);
        modeData = (CarShopGameModeData)gameConfig.GameModeData;

        var spawnConfig = modeData.carSpawnConfig;
        CityEntitiesCreationHelper.CreateNewCar(
            spawnConfig.CarBaseConfig,
            spawnConfig.CarVariantConfig,
            G.City.GetClosestPosition(modeData.carSpawnPoint));

    }
    public override void InitializeLogic(GameConfig gameConfig)
    {
        base.InitializeLogic(gameConfig);

        var roamingState = new FreeRoamGameState();
        G.GameFlowController.SetGameState(roamingState);
    }

    public override void AfterSceneLoad(ISceneMain sceneMain)
    {
        base.AfterSceneLoad(sceneMain);
        FirstLoadActions();
    }

    void FirstLoadActions()
    {
        if (firstLoaded)
            return;
        firstLoaded = true;

        //CollectablesManager.Instance.Initialize(locations, 900f, null, 20);
        //PoliceManager.Instance.CreatePolice();
        //tryEnterCarShop(car);
    }

    void tryEnterCarShop(Car car)
    {
        var carShopWarehouse = World.Instance.WorldRegistry.GetByConfig<Warehouse>(modeData.carShopWarehouseConfig)?.First();
        if (carShopWarehouse == null)
        {
            Debug.LogError("CarShop warehouse not found in WorldRegistry.");
            return;
        }

        var result = G.TransactionProcessor.Process(new PutProductsInHolderTransaction(new WarehouseHolderAdapter(carShopWarehouse),
            car));

        if (result.Type == TransactionResultType.Success)
        {
            G.GameFlowController.EnterWarehouse(carShopWarehouse);
        }
        else
            Debug.LogError("Failed to put car inside carshop warehouse: " + result);
    }
}