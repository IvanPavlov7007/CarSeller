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

        var roamingState = new FreeRoamGameState();
        G.GameFlowController.SetGameState(roamingState);

        G.VehicleController.Initialize(gameConfig.VehicleControllerConfig);
        G.CityVision.Initialize();
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
    }
}