using UnityEngine;

public static class CameraHelper
{
    public static void SetCurrentPositionAtCar()
    {
        var cityEntity = G.VehicleController.CurrentVehicleEntity;
        Debug.Assert(CameraMovementManager.Instance != null);
        CameraMovementManager.Instance?.Teleport(cityEntity.Position.WorldPosition);
    }

    private static Vector2 getCityLocationPosition(CityEntity cityEntity)
    {
        if(cityEntity != null)
        {
            return cityEntity.Position.WorldPosition;
        }
        Debug.LogError("CityLocation is null in ViewEnsure");
        return Vector2.zero;
    }
}