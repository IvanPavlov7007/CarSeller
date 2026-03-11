using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VehicleControllerConfig", menuName = "Configs/Player/VehicleControllerConfig")]
public class VehicleShopConfig : CityLocatedConfig
{
    public List<SimplifiedCarIdentifier> allCarOptions;
    public List<SimplifiedCarIdentifier> initiallyUnlockedOptions;
}