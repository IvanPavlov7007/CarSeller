using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class DebugProductSpawner : MonoBehaviour
{
    public WarehouseSceneManager warehouseManager;

    public CarBaseConfig carBaseConfig;
    public CarVariantConfig carVariantConfig;

    public WheelBaseConfig wheelBaseConfig;
    public WheelVariantConfig wheelVariantConfig;

    public EngineBaseConfig engineBaseConfig;
    public EngineVariantConfig engineVariantConfig;

    public SpoilerBaseConfig spoilerBaseConfig;
    public SpoilerVariantConfig spoilerVariantConfig;

    public CarFrameBaseConfig carFrameBaseConfig;
    public CarFrameVariantConfig carFrameVariantConfig;

    [Button]
    public void SpawnCar()
    {
        G.Instance.ProductManager.CreateCar(
            carBaseConfig,
            carVariantConfig,
            WarehouseSceneManager.Instance.Warehouse.GetEmptyLocation() );
    }

    [Button]
    public void SpawnWheel()
    {
        G.Instance.ProductManager.CreateWheel(
            wheelBaseConfig,
            wheelVariantConfig,
            WarehouseSceneManager.Instance.Warehouse.GetEmptyLocation() );
    }
    [Button]
    public void SpawnEngine()
    {
        G.Instance.ProductManager.CreateEngine(
            engineBaseConfig,
            engineVariantConfig,
            WarehouseSceneManager.Instance.Warehouse.GetEmptyLocation() );
    }
    [Button]
    public void SpawnSpoiler()
    {
        G.Instance.ProductManager.CreateSpoiler(
            spoilerBaseConfig,
            spoilerVariantConfig,
            WarehouseSceneManager.Instance.Warehouse.GetEmptyLocation() );
    }

}