using UnityEngine;

public static class CitySceneBootstrap
{
    public static void Execute()
    {
        GameMain.Instance.OnCityInitialize();
    }
}