using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarsDefinitionLibrary", menuName = "Configs/CarsDefinitionLibrary", order = 0)]
public class CarsDefinitionLibrary : SerializedScriptableObject
{
    [OdinSerialize] List<CarBaseConfig> mainBaseConfigs = new List<CarBaseConfig>();

    NonDuplicatesIdentifiedCarList<List<CarRuntimeConfig>> runtimeConfigs;
    public IReadOnlyNonDuplicatesIdentifiedCarList<List<CarRuntimeConfig>> RuntimeConfigs => runtimeConfigs;

    public void Initialize()
    {
        initializeRuntimesMatrix();
    }

    private void initializeRuntimesMatrix()
    {
        CarConfigResolver resolver = new CarConfigResolver();
        runtimeConfigs = new NonDuplicatesIdentifiedCarList<List<CarRuntimeConfig>>();

        foreach (var mainBaseConfig in mainBaseConfigs)
        {
            insertRuntimeIntoAllConfigs(resolver.Resolve(mainBaseConfig,null));
        }
    }

    private void insertRuntimeIntoAllConfigs(CarRuntimeConfig runtimeConfig)
    {
        var identifier = runtimeConfig.Kind;
        if (runtimeConfigs.TryGetValue(identifier, out var runtimesList))
        {
            runtimesList.Add(runtimeConfig);
        }
        else
        {
            runtimeConfigs.Add(identifier, new List<CarRuntimeConfig>() { runtimeConfig });
        }
    }
}