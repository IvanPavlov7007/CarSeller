using UnityEngine;
using Pixelplacement;
using System.Collections;

public static class CameraHelper
{
    public static void SetCurrentPositionAtCar()
    {
        var car = G.GameState.FocusedCar;
        var cityEntity = CityLocatorHelper.GetCityEntity(car);
        Debug.Assert(CameraMovementManager.Instance != null);
        CameraMovementManager.Instance?.Teleport(cityEntity.Position.WorldPosition);
    }

    public static void SetCurrentPositionAtPlayerFigure()
    {
        var figure = G.GameState.PlayerFigure;
        if (figure == null)
        {
            Debug.LogWarning("CameraHelper.SetCurrentPositionAtPlayerFigure: PlayerFigure is null");
            return;
        }

        var cityEntity = CityLocatorHelper.GetCityEntity(figure);
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