using UnityEngine;

[CreateAssetMenu(fileName = "CityConfig", menuName = "Configs/CityConfig")]
public class CityConfig : ScriptableObject
{
    public string cityName;
    public string SceneToLoad;

    public Vector2 cityLeftUpPos;
    public int gridNodesCountX;
    public int gridNodesCountY;
    public float gridNodeWorldSize;
    public CityGraphAsset CityGraph;

    public WarehouseConfig[] warehouseConfigs;
    public SimpleProductSpawnConfig[] initialProductsToSpawn;
    
}