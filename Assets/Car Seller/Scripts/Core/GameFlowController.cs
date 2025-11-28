
public class GameFlowController
{
    public void EnterWarehouse(Warehouse warehouse)
    {
        G.Instance.InteractionManager = new WarehouseInteractionManager();
    }
}