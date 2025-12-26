using Pixelplacement;

public class CarSpawnWhenCityClosed : Singleton<CarSpawnWhenCityClosed>
{
    private void Start()
    {
        CarSpawnManager.CheckAndRefill();
    }
}