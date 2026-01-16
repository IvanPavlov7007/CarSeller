using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/Game Config")]
public class GameConfig : ScriptableObject
{
    public GameConfigMode GameConfigMode;
    public CityConfig CityConfig;
    public EconomyConfig EconomyConfig;
    public WorldMissionsConfig WorldMissionsConfig;
}