using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SimplifiedCarsCreationBuilder", menuName = "Configs/SimplifiedCarsCreationBuilder", order = 0)]
public class SimplifiedCarsCreationBuilder : SerializedScriptableObject
{
    [OdinSerialize] Dictionary<(CarType, CarRarity), CarBaseConfig> baseConfigs = new Dictionary<(CarType, CarRarity), CarBaseConfig>();
    [OdinSerialize] Dictionary<CarColor, CarVariantConfig> variantConfigs = new Dictionary<CarColor, CarVariantConfig>();
    [OdinSerialize] List<CarBaseConfig> mainBaseConfigs = new List<CarBaseConfig>();

    NonDuplicatesIdentifiedCarList<CarRuntimeConfig> runtimeConfigs;
    public IReadOnlyNonDuplicatesIdentifiedCarList<CarRuntimeConfig> RuntimeConfigs => runtimeConfigs;

    public void Initialize()
    {
        initializeRuntimesMatrix();
    }

    private void initializeRuntimesMatrix()
    {
        CarConfigResolver resolver = new CarConfigResolver();
        runtimeConfigs = new NonDuplicatesIdentifiedCarList<CarRuntimeConfig>();

        foreach (var baseConfig in baseConfigs)
        {
            foreach (var variantConfig in variantConfigs)
            {
                var identifier = new SimplifiedCarIdentifier(baseConfig.Key.Item1, baseConfig.Key.Item2, variantConfig.Key);
                runtimeConfigs.Add(identifier, resolver.Resolve(baseConfig.Value, variantConfig.Value));
            }
        }

        foreach (var mainBaseConfig in mainBaseConfigs)
        {
                var identifier = new SimplifiedCarIdentifier(mainBaseConfig.Type, mainBaseConfig.Rarity, mainBaseConfig.Color);
                runtimeConfigs.Add(identifier, resolver.Resolve(mainBaseConfig, null));
        }
    }

    /*
     * TODO validate:
     * 1) all combinations of (CarType, CarRarity) and CarColor are present in the dictionaries
     * 2) car runtime configs values do match the identifiers (e.g. a runtime config with CarType=Truck should not be created from a base config with CarType=Sport)
     */
}