using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugCommands : MonoBehaviour
{
#if UNITY_EDITOR || DEBUG
    
    public void OnA(InputValue val)
    {

        G.CarSpawnManager.SpawnCarAtPosition(G.VehicleController.CurrentVehicleEntity.Position);
    }

    public void OnD(InputValue val)
    {

    }

    public void OnL(InputValue val)
    {
        Debug.Log("Cheat visible: " + !G.City.AspectsSystem.visibleSystem.cheatVisible);
        G.City.AspectsSystem.visibleSystem.cheatVisible = !G.City.AspectsSystem.visibleSystem.cheatVisible;
    }

    public void OnS(InputValue val)
    {
        G.BuyerManager.SpawnBuyerAtPosition(Buyer.Any(), G.VehicleController.CurrentVehicleEntity.Position, null,null);
    }

    public void OnW(InputValue val)
    {

    }

    public void OnQ(InputValue val)
    {
        CarFlexibleJunctionPolicy.IgnoreRules = !CarFlexibleJunctionPolicy.IgnoreRules;
        //var pos = G.VehicleController.CurrentVehicleEntity.Position;
        //Car car = G.SimplifiedCarsManager.CreateCarHidden(new CarKind(CarType.Bike, CarRarity.Common));
        //CityEntitiesCreationHelper.MoveInExistingCar(car, pos);
    }


#endif
}