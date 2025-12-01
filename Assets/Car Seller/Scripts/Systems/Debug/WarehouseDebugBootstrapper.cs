using UnityEngine;

public class WarehouseDebugBootstrapper : MonoBehaviour
{
    private void Start()
    {
        G.Instance.GameFlowController.EnterWarehouse(WarehouseSceneManager.SceneWarehouseModel);
    }
}