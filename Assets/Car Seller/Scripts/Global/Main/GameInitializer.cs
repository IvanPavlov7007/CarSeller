using System.Collections.Generic;

public static class GameInitializer
{
    static G G => G.Instance;

    public static void Initialize()
    {
        GameEvents.Instance.Reset();
        InitializeGameObjects();
        ResetGameState();
    }

    private static void InitializeGameObjects()
    {
        G.GameFlowManager = new GameFlowManager();
        G.CarMechanicService = new CarMechanicService();
    }

    private static void ResetGameState()
    {
        G.WorldManager.InitializeWorld(G.CityConfig, G.EconomyConfig);
    }
}