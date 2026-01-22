using System;
using UnityEngine;
using static GameFlowController;

public abstract partial class GameMain
{
    public sealed class GameMainResolover
    {
        public GameMain Resolve(GameConfig gameConfig)
        {
            switch (gameConfig.GameConfigMode)
            {
                case GameConfigMode.CarShop:
                    return new CarShopGameMain();
                case GameConfigMode.CarSteal:
                    return new CarStealGameMain();
                case GameConfigMode.DisassembleStolenCars:
                    return new DisassembleStolenCarsGameMain();
                default:
                    Debug.LogError("GameMainReslover.Resolve: Unsupported GameConfigMode!");
                    return null;
            }
        }
    }
    public sealed class SceneMainResolver
    {
        public ISceneMain Resolve(SceneEntrancePoint sceneEntrancePoint)
        {
            Debug.Assert(sceneEntrancePoint != null);

            switch (sceneEntrancePoint.gameSceneType)
            {
                case GameSceneType.City:
                    return new CitySceneMain();
                    break;
                case GameSceneType.Warehouse:
                    var name = sceneEntrancePoint.specificName();
                    var warehouse = World.Instance.WorldRegistry.GetByName<Warehouse>(name);
                    Debug.Assert(warehouse != null, $"GameFlowController.Initialize: Warehouse with id {name} not found!");
                    setWarehouse(warehouse);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

public interface ISceneMain
{
    void InitializeSceneView();
    void InitializeSceneLogic();
}