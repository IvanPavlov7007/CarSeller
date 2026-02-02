using UnityEngine;
using Pixelplacement;
using System.Collections;

public static class CameraHelper
{
    public static void SetCurrentPositionAtCar()
    {
        var car = G.GameState.FocusedCar;
        var location = CityLocatorHelper.GetCityLocation(car);
        Debug.Assert(CameraMovementManager.Instance != null);
        CameraMovementManager.Instance?.Teleport(getCityLocationPosition(location));
    }

    private static Vector2 getCityLocationPosition(City.CityLocation cityLocation)
    {
        if(cityLocation != null)
        {
            return cityLocation.CityPosition.WorldPosition;
        }
        Debug.LogError("CityLocation is null in ViewEnsure");
        return Vector2.zero;
    }
}