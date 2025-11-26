
using Pixelplacement;
using UnityEngine;

public class G : Singleton<G>
{
    public LocationService LocationService;
    public ProductManager ProductManager;

    private void Awake()
    {
        LocationService = new LocationService();
        ProductManager = new ProductManager();
        ResetGameState();
    }

    public void ResetGameState()
    {
        World.Reset();
        GameEvents.Instance.Reset();
    }

}
