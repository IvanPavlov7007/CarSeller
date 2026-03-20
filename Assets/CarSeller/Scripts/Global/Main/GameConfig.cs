using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/Game Config")]
public class GameConfig : SerializedScriptableObject
{
    public GameConfigMode GameConfigMode;
    public CityConfig CityConfig;
    public EconomyConfig EconomyConfig;
    public WorldMissionsConfig WorldMissionsConfig;
    public VehicleControllerConfig VehicleControllerConfig;
    public GameDatabaseContainer GameDatabaseContainer;
    [Header("Game Modes Data")]
    public ScriptableObject GameModeData;
}