using UnityEngine;

[CreateAssetMenu(fileName = "SimpleCarSpawnConfig", menuName = "Configs/Spawn/CarSpawnConfig")]
public class SimpleCarSpawnConfig : ScriptableObject
{
    public CarBaseConfig CarBaseConfig;
    public CarVariantConfig CarVariantConfig;

    public Car GenerateCar(ILocation location)
    {
        Car car = G.ProductManager.CreateCar(
                    CarBaseConfig,
                    CarVariantConfig,
                    location);
        return car;
    }
}