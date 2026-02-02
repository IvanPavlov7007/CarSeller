using UnityEngine;

public abstract partial class GameMain
{
    public class WarehouseSceneMain : ISceneMain
    {
        private Warehouse warehouse;
        public WarehouseSceneMain(SceneEntrancePoint sceneEntrancePoint)
        {
            var name = sceneEntrancePoint.specificName();
            warehouse = World.Instance.WorldRegistry.GetByName<Warehouse>(name);
            Debug.Assert(warehouse != null, $"GameFlowController.Initialize: Warehouse with id {name} not found!");
        }
        public void Enter()
        {
            G.GameFlowController.SetWarehouse(warehouse);
            WarehouseSceneManager.Instance.InitializeWarehouse();
        }

        public void Exit()
        {
            // No specific exit logic for warehouse scene
        }

    }
}