using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PersonalVehicleShopConfig", menuName = "Configs/Player/PersonalVehicleShopConfig")]
public class VehicleShopConfig : CityLocatedConfig
{
    public List<CarKind> allCarOptions;
    public List<CarKind> initiallyUnlockedOptions;
}