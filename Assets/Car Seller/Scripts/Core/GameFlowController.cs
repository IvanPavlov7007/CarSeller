
using Pixelplacement;

public class GameFlowController
{
    public void EnterWarehouse(Warehouse warehouse)
    {
        WarehouseSceneManager.SceneWarehouseModel = warehouse;
        G.Instance.InteractionManager = new WarehouseInteractionManager();
    }

    //TODO add building context and stuff

    public void RideCar(Car car)
    {
        
    }
}