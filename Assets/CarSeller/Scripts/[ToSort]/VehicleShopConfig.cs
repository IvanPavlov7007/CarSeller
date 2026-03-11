using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PersonalVehicleShopConfig", menuName = "Configs/Player/PersonalVehicleShopConfig")]
public class VehicleShopConfig : CityLocatedConfig
{
    public List<SimplifiedCarIdentifier> allCarOptions;
    public List<SimplifiedCarIdentifier> initiallyUnlockedOptions;
}