using UnityEngine;
using Pixelplacement;
using System.Collections;

public class EnsureCameraPositionOnStart : Singleton<EnsureCameraPositionOnStart>
{
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        var car = G.GameState.FocusedCar;
        var location = CityLocatorHelper.GetCityLocation(car);
        Debug.Log($"Ensuring camera position on start for car {car.Name} at location {location?.CityPosition}");
        CameraMovementManager.Instance?.Teleport(getCityLocationPosition(location));
    }

    private Vector2 getCityLocationPosition(City.CityLocation cityLocation)
    {
        if(cityLocation != null)
        {
            return cityLocation.CityPosition.WorldPosition;
        }
        Debug.LogError("CityLocation is null in ViewEnsure");
        return Vector2.zero;
    }
}