using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WarehouseConfig", menuName = "Configs/WarehouseConfig")]
public class WarehouseConfig : ScriptableObject
{
    public string DisplayName;

    public string SceneToLoad;

    //Current Mockup Parameters
    public Vector2 warehouseClosestInitialPosition;
    public SimpleProductSpawnConfig[] initialProductsToSpawn;
}

[Serializable]
public class SimpleProductSpawnConfig
{
    public IBaseConfig productBaseConfig;
    public ISimpleVariantConfig productVariantConfig;
}