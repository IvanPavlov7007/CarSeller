using System.Collections.Generic;
using System.Linq;

public class SimplifiedGameplayMain : GameMain
{
    bool firstLoaded = false;
    PersonalVehicleShop shop;

    public override void InitializeWorld(GameConfig gameConfig)
    {
        base.InitializeWorld(gameConfig);
        G.CarSpawnManager.SubscribeToEvents();
        initializeCityAreas();
        createShop(gameConfig.VehicleControllerConfig.vehicleShopConfig);
    }

    private void initializeCityAreas()
    {
        G.Areas = G.Balancing.AreaBalancingByAreaName.
            ToDictionary(area => area.Id, area => new CityArea(area));
    }

    public override void InitializeLogic(GameConfig gameConfig)
    {
        base.InitializeLogic(gameConfig);

        var roamingState = new FreeRoamGameState();
        G.GameFlowController.SetGameState(roamingState);

        G.VehicleController.Initialize(gameConfig.VehicleControllerConfig, shop.PersonalVehiclesList);
    }

    private void createShop(VehicleShopConfig config)
    {
        shop = new PersonalVehicleShop(config);
        var pos = config.Marker.GetCityPosition();
        CityEntitiesCreationHelper.CreatePersonalVehicleShop(shop, pos);
    }

    public override void AfterSceneLoad(ISceneMain sceneMain)
    {
        base.AfterSceneLoad(sceneMain);

        FirstLoadActions();

        //if (sceneMain is CitySceneMain)
            //G.CarSpawnManager.CheckAndRefill();
    }

    void FirstLoadActions()
    {
        if (firstLoaded)
            return;
        firstLoaded = true;
    }
}