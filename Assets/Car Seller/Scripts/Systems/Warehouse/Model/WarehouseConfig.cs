using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "WarehouseConfig", menuName = "Configs/Economy/WarehouseConfig")]
public class WarehouseConfig : SerializedScriptableObject
{
    [InfoBox("Used as WarehouseIdentifier and the same as the scene name")]
    public string Name;

    public Sprite image;
    public string SizeCategory = "Small garage";
    public int CarParkingSpots = 5;

    [Title("Placement")]
    [InlineProperty, HideLabel]
    public CityMarkerRef Marker;

    [HideInInspector]
    public Vector2 warehouseClosestInitialPosition;

    public SimpleProductSpawnConfig[] initialProductsToSpawn;
}

[Serializable]
public class SimpleProductSpawnConfig
{
    public ScriptableObject productBaseConfig;
    public ScriptableObject productVariantConfig;
}