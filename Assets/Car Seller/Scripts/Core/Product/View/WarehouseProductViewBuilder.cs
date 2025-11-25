
using UnityEngine;

public class WarehouseProductViewBuilder : IProductViewBuilder<GameObject>
{
    public GameObject BuildCar(Car car)
    {
        throw new System.NotImplementedException();
    }

    public GameObject BuildCarFrame(CarFrame carFrame)
    {
        Debug.LogError("WarehouseProductViewBuilder does not support building CarFrame views. " + carFrame.UniqueName);
        return null;
    }

    public GameObject BuildEngine(Engine engine)
    {
        throw new System.NotImplementedException();
    }

    public GameObject BuildSpoiler(Spoiler spoiler)
    {
        throw new System.NotImplementedException();
    }

    public GameObject BuildWheel(Wheel wheel)
    {
        throw new System.NotImplementedException();
    }
}