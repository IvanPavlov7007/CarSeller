using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WarehouseConfig", menuName = "Configs/WarehouseConfig")]
public class WarehouseConfig : ScriptableObject
{
    public string DisplayName;

    public string SceneToLoad;

    [Tooltip("Optional marker ID to place this warehouse. Falls back to 'warehouseClosestInitialPosition' if empty.")]
    public string CityMarkerId;

    //Current Mockup Parameters
    public Vector2 warehouseClosestInitialPosition;
    public SimpleProductSpawnConfig[] initialProductsToSpawn;
}

[Serializable]
public class SimpleProductSpawnConfig
{
    public ScriptableObject productBaseConfig;
    public ScriptableObject productVariantConfig;
}