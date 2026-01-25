using System;
using UnityEngine;
using static GameFlowController;

public abstract partial class GameMain
{
    public sealed class GameMainResolver
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
                    Debug.LogError("GameMainResolver.Resolve: Unsupported GameConfigMode!");
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
                case GameSceneType.Warehouse:
                    return new WarehouseSceneMain(sceneEntrancePoint);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

public interface ISceneMain
{
    void Enter();
    void Exit();
}