using UnityEngine;

[CreateAssetMenu(fileName = "CityConfig", menuName = "Configs/CityConfig")]
public class CityConfig : ScriptableObject
{
    public string cityName;
    public WarehouseConfig[] warehouseConfigs;
    public SimpleProductSpawnConfig[] initialProductsToSpawn;
}