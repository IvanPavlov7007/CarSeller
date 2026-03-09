public class SimplifiedGameplayMain : GameMain
{
    bool firstLoaded = false;
    public override void InitializeWorld(GameConfig gameConfig)
    {
        base.InitializeWorld(gameConfig);
        G.CarSpawnManager.SubscribeToEvents();
        G.CarSpawnManager.NewCarsRotation();
    }
    public override void InitializeLogic(GameConfig gameConfig)
    {
        base.InitializeLogic(gameConfig);

        var car =
            CityLocatorHelper.GetClosestCar(World.Instance.WorldRegistry.GetByConfig<Warehouse>(gameConfig.CityConfig.warehouseConfigs[0])[0], out _);

        var roamingState = new FreeRoamGameState(car);
        G.GameFlowController.SetGameState(roamingState);
    }

    public override void AfterSceneLoad(ISceneMain sceneMain)
    {
        base.AfterSceneLoad(sceneMain);

        FirstLoadActions();

        if (sceneMain is CitySceneMain)
            G.CarSpawnManager.CheckAndRefill();
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
}